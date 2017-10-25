package io.pivotal.om.controller;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.cloud.client.ServiceInstance;
import org.springframework.cloud.client.discovery.DiscoveryClient;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.*;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.client.RestTemplate;

import io.pivotal.om.domain.ExecutionReport;
import io.pivotal.om.repository.OrderRepository;
@CrossOrigin(origins = "*", maxAge = 3600)
@RestController
@Configuration
@EnableAutoConfiguration
public class UIServices {
	
	Logger logger = LoggerFactory.getLogger(UIServices.class);
	
	private OrderRepository or;
	private RestTemplate restTemplate;
	private DiscoveryClient discoveryClient;

	@Value("${config.rate}")
	int rate;


	@Autowired
	public UIServices(OrderRepository or, RestTemplate restTemplate, DiscoveryClient discoveryClient) {
		this.or = or;
		this.restTemplate = restTemplate;
		this.discoveryClient = discoveryClient;
	}

	@DeleteMapping(value="api/client/{clientId}/order/{orderId}")
	public ExecutionReport deleteOrder(@PathVariable String clientId, @PathVariable String orderId) {
		ExecutionReport order = or.findOne(orderId);
		String symbol = order.getSymbol();
		String url = lookupUrlForExchange(symbol) + "/api/order/" + String.valueOf(orderId);
		ResponseEntity<ExecutionReport> re = restTemplate.exchange(url, HttpMethod.DELETE, null, ExecutionReport.class);
		ExecutionReport eor = re.getBody();
		or.save(eor);
		or.flush();
		return eor;
	}

	@RequestMapping(value="api/client/{clientId}/orders", method=RequestMethod.GET)
	public List<ExecutionReport> getOrders(@PathVariable String clientId) {
		List<ExecutionReport> clientOrders = or.ordersByClient(clientId);
		return clientOrders;
	}


	@RequestMapping(value="/api/exchanges", method=RequestMethod.GET)
	public List<String> getExchanges() {
		
		List<String> services = discoveryClient.getServices();
		List<String> exchanges = new ArrayList<String>();
		for (String service : services) {
			if(service.toUpperCase().startsWith("EXCHANGE_"))
				exchanges.add(service.substring("EXCHANGE_".length()).trim().toUpperCase());
		}
		return exchanges;
	}


	@PostMapping(value="api/order")
	@Transactional
	@ResponseBody
	public ExecutionReport placeOrder(@RequestBody ExecutionReport clientOrderRequest) {
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

	  private String lookupUrlForExchange(String symbol) {
//		  return "http://exchange-btcusd.apps.pcf.guru";
		  List<ServiceInstance> serviceInstances = discoveryClient.getInstances("Exchange_" + symbol);
		  String url = serviceInstances.get(0).getUri().toString();
		  return url;
	  }
		
}
