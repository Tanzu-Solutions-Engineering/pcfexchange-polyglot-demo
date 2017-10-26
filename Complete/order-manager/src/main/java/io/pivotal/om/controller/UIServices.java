package io.pivotal.om.controller;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import io.pivotal.om.domain.ExchangeService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.cloud.client.ServiceInstance;
import org.springframework.cloud.client.circuitbreaker.EnableCircuitBreaker;
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
	private DiscoveryClient discoveryClient;
	private ExchangeService exchangeService;

	@Autowired
	public UIServices(OrderRepository or, DiscoveryClient discoveryClient, ExchangeService exchangeService) {
		this.or = or;
		this.discoveryClient = discoveryClient;

		this.exchangeService = exchangeService;
	}

	@DeleteMapping(value="api/client/{clientId}/order/{orderId}")
	public ExecutionReport deleteOrder(@PathVariable String clientId, @PathVariable String orderId) {
		return exchangeService.deleteOrder(clientId, orderId);
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
	@ResponseBody
	public ExecutionReport placeOrder(@RequestBody ExecutionReport clientOrderRequest) {
		return exchangeService.placeOrder(clientOrderRequest);
	}


}
