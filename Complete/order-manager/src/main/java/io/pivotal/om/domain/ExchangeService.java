package io.pivotal.om.domain;

import com.netflix.hystrix.contrib.javanica.annotation.HystrixCommand;
import io.pivotal.om.controller.UIServices;
import io.pivotal.om.repository.OrderRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.cloud.client.ServiceInstance;
import org.springframework.cloud.client.discovery.DiscoveryClient;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.client.RestTemplate;


import java.util.HashMap;
import java.util.List;

@Service
@Configuration
@EnableAutoConfiguration

public class ExchangeService {
    private final RestTemplate restTemplate;
    private DiscoveryClient discoveryClient;
    Logger logger = LoggerFactory.getLogger(UIServices.class);
    private OrderRepository or;

    public ExchangeService(OrderRepository or, RestTemplate restTemplate, DiscoveryClient discoveryClient) {
        this.or = or;
        this.restTemplate = restTemplate;
        this.discoveryClient = discoveryClient;
    }
    @Value("${config.rate}")
    int rate;

    @Transactional
    @ResponseBody
    @HystrixCommand(fallbackMethod = "placeOrderFallback")
    public ExecutionReport placeOrder(ExecutionReport clientOrderRequest) {
        String orderId = java.util.UUID.randomUUID().toString();
        clientOrderRequest.setOrderId(orderId);
        logger.debug("Created new order with ID=" + orderId);
        String url = lookupUrlForExchange(clientOrderRequest.getSymbol()) + "/api/order/" + String.valueOf(orderId);
        logger.debug("Exchange service URL=" + url);

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<ExecutionReport> httpOrderRequest = new HttpEntity<>(clientOrderRequest, headers);
        ResponseEntity<ExecutionReport[]> re = restTemplate.exchange(url, HttpMethod.PUT, httpOrderRequest, ExecutionReport[].class);

        ExecutionReport[] eor = re.getBody();
        HashMap<String,ExecutionReport> ordersToSave = new HashMap<>();

        for(ExecutionReport er : eor)
        {
            er.setLastCommission(rate);
            ordersToSave.put(er.getOrderId(), er);
        }
        ExecutionReport newOrderLastState = ordersToSave.get(orderId);
        newOrderLastState.setNew(true);
        ordersToSave.forEach((l, order) -> or.save(order));
        or.flush();

        return newOrderLastState;
    }

    public ExecutionReport deleteOrder(String clientId, String orderId) {
        ExecutionReport order = or.findOne(orderId);
        String symbol = order.getSymbol();
        String url = lookupUrlForExchange(symbol) + "/api/order/" + String.valueOf(orderId);
        ResponseEntity<ExecutionReport> re = restTemplate.exchange(url, HttpMethod.DELETE, null, ExecutionReport.class);
        ExecutionReport eor = re.getBody();
        or.save(eor);
        or.flush();
        return eor;
    }
    public ExecutionReport placeOrderFallback(ExecutionReport clientOrderRequest) {
        clientOrderRequest.setExecType("Rejected");
        return clientOrderRequest;
    }

    private String lookupUrlForExchange(String symbol) {
//		  return "http://exchange-btcusd.apps.pcf.guru";
        List<ServiceInstance> serviceInstances = discoveryClient.getInstances("Exchange_" + symbol);
        String url = serviceInstances.get(0).getUri().toString();
        return url;
    }
}
