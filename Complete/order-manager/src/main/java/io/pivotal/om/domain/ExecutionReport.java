package io.pivotal.om.domain;

import com.fasterxml.jackson.annotation.JsonInclude;
import org.springframework.data.domain.Persistable;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Transient;

@Entity
@JsonInclude(JsonInclude.Include.NON_NULL)
public class ExecutionReport implements Persistable<String> {

	public void setNew(boolean aNew) {
		isNew = aNew;
	}

	@Transient
	private boolean isNew;
	private String execType;
	private String execId;
	private String tradeId;
	private String execRefID;
	private int totalNumReports;
	private String clOrdID;
	private String ordStatusReqID;
	private String origClOrdID;
	private String clientID;
	private String massStatusReqID;
	@Id
	private String orderId;
	private long secondaryOrderId;
	private String symbol;
	private int price;
	private int stopPx;
	private int orderQty;
	private String ordType;
	private String side;
	private String timeInForce;
	private String pegScope;
	private String pegPriceType;
	private long pegOffset;
	private long seqNum;
	private String triggerPriceType;
	private int lastPx;
	private int lastQty;
	private int cumQty;
	private int avgPx;
	private int leavesQty;
	private String transactTime;
	private long fee;
	private int lastCommission;
	private int cummCommission;
	private String trdMatchID;
	private String ordStatus;
	private long origEscrow;
	private long leavesEscrow;
	private int peggedPrice;
	private String lastLiquidityInd;
	private String submitTime;
	private String ordRejReason;
	private String cxlRejReason;
	private boolean escrowRestricted;

	public ExecutionReport() {
		super();
		// TODO Auto-generated constructor stub
	}

	public ExecutionReport(String execType, String execId, String tradeId, String execRefID, int totalNumReports, String clOrdID, String ordStatusReqID, String origClOrdID, String clientID, String massStatusReqID, String orderId, long secondaryOrderId, String symbol, int price, int stopPx, int orderQty, String ordType, String side, String timeInForce, String pegScope, String pegPriceType, long pegOffset, long seqNum, String triggerPriceType, int lastPx, int lastQty, int cumQty, int avgPx, int leavesQty, String transactTime, long fee, int lastCommission, int cummCommission, String trdMatchID, String ordStatus, long origEscrow, long leavesEscrow, int peggedPrice, String lastLiquidityInd, String submitTime, String ordRejReason, String cxlRejReason, boolean escrowRestricted) {
		this.execType = execType;
		this.execId = execId;
		this.tradeId = tradeId;
		this.execRefID = execRefID;
		this.totalNumReports = totalNumReports;
		this.clOrdID = clOrdID;
		this.ordStatusReqID = ordStatusReqID;
		this.origClOrdID = origClOrdID;
		this.clientID = clientID;
		this.massStatusReqID = massStatusReqID;
		this.orderId = orderId;
		this.secondaryOrderId = secondaryOrderId;
		this.symbol = symbol;
		this.price = price;
		this.stopPx = stopPx;
		this.orderQty = orderQty;
		this.ordType = ordType;
		this.side = side;
		this.timeInForce = timeInForce;
		this.pegScope = pegScope;
		this.pegPriceType = pegPriceType;
		this.pegOffset = pegOffset;
		this.seqNum = seqNum;
		this.triggerPriceType = triggerPriceType;
		this.lastPx = lastPx;
		this.lastQty = lastQty;
		this.cumQty = cumQty;
		this.avgPx = avgPx;
		this.leavesQty = leavesQty;
		this.transactTime = transactTime;
		this.fee = fee;
		this.lastCommission = lastCommission;
		this.cummCommission = cummCommission;
		this.trdMatchID = trdMatchID;
		this.ordStatus = ordStatus;
		this.origEscrow = origEscrow;
		this.leavesEscrow = leavesEscrow;
		this.peggedPrice = peggedPrice;
		this.lastLiquidityInd = lastLiquidityInd;
		this.submitTime = submitTime;
		this.ordRejReason = ordRejReason;
		this.cxlRejReason = cxlRejReason;
		this.escrowRestricted = escrowRestricted;
	}

	public String getExecType() {
		return execType;
	}
	public void setExecType(String execType) {
		this.execType = execType;
	}
	public String getExecId() {
		return execId;
	}
	public void setExecId(String execId) {
		this.execId = execId;
	}
	public String getTradeId() {
		return tradeId;
	}
	public void setTradeId(String tradeId) {
		this.tradeId = tradeId;
	}
	public String getExecRefID() {
		return execRefID;
	}
	public void setExecRefID(String execRefID) {
		this.execRefID = execRefID;
	}
	public int getTotalNumReports() {
		return totalNumReports;
	}
	public void setTotalNumReports(int totalNumReports) {
		this.totalNumReports = totalNumReports;
	}
	public String getClOrdID() {
		return clOrdID;
	}
	public void setClOrdID(String clOrdID) {
		this.clOrdID = clOrdID;
	}
	public String getOrdStatusReqID() {
		return ordStatusReqID;
	}
	public void setOrdStatusReqID(String ordStatusReqID) {
		this.ordStatusReqID = ordStatusReqID;
	}
	public String getOrigClOrdID() {
		return origClOrdID;
	}
	public void setOrigClOrdID(String origClOrdID) {
		this.origClOrdID = origClOrdID;
	}
	public String getClientID() {
		return clientID;
	}
	public void setClientID(String clientID) {
		this.clientID = clientID;
	}
	public String getMassStatusReqID() {
		return massStatusReqID;
	}
	public void setMassStatusReqID(String massStatusReqID) {
		this.massStatusReqID = massStatusReqID;
	}
	public String getOrderId() {
		return orderId;
	}
	public void setOrderId(String orderId) {
		this.orderId = orderId;
	}
	public long getSecondaryOrderId() {
		return secondaryOrderId;
	}
	public void setSecondaryOrderId(long secondaryOrderId) {
		this.secondaryOrderId = secondaryOrderId;
	}
	public String getSymbol() {
		return symbol;
	}
	public void setSymbol(String symbol) {
		this.symbol = symbol;
	}
	public int getPrice() {
		return price;
	}
	public void setPrice(int price) {
		this.price = price;
	}
	public int getStopPx() {
		return stopPx;
	}
	public void setStopPx(int stopPx) {
		this.stopPx = stopPx;
	}
	public int getOrderQty() {
		return orderQty;
	}
	public void setOrderQty(int orderQty) {
		this.orderQty = orderQty;
	}
	public String getOrdType() {
		return ordType;
	}
	public void setOrdType(String ordType) {
		this.ordType = ordType;
	}
	public String getSide() {
		return side;
	}
	public void setSide(String side) {
		this.side = side;
	}
	public String getTimeInForce() {
		return timeInForce;
	}
	public void setTimeInForce(String timeInForce) {
		this.timeInForce = timeInForce;
	}
	public String getPegScope() {
		return pegScope;
	}
	public void setPegScope(String pegScope) {
		this.pegScope = pegScope;
	}
	public String getPegPriceType() {
		return pegPriceType;
	}
	public void setPegPriceType(String pegPriceType) {
		this.pegPriceType = pegPriceType;
	}
	public long getPegOffset() {
		return pegOffset;
	}
	public void setPegOffset(long pegOffset) {
		this.pegOffset = pegOffset;
	}
	public long getSeqNum() {
		return seqNum;
	}
	public void setSeqNum(long seqNum) {
		this.seqNum = seqNum;
	}
	public String getTriggerPriceType() {
		return triggerPriceType;
	}
	public void setTriggerPriceType(String triggerPriceType) {
		this.triggerPriceType = triggerPriceType;
	}
	public int getLastPx() {
		return lastPx;
	}
	public void setLastPx(int lastPx) {
		this.lastPx = lastPx;
	}
	public int getLastQty() {
		return lastQty;
	}
	public void setLastQty(int lastQty) {
		this.lastQty = lastQty;
	}
	public int getCumQty() {
		return cumQty;
	}
	public void setCumQty(int cumQty) {
		this.cumQty = cumQty;
	}
	public int getAvgPx() {
		return avgPx;
	}
	public void setAvgPx(int avgPx) {
		this.avgPx = avgPx;
	}
	public int getLeavesQty() {
		return leavesQty;
	}
	public void setLeavesQty(int leavesQty) {
		this.leavesQty = leavesQty;
	}
	public String getTransactTime() {
		return transactTime;
	}
	public void setTransactTime(String transactTime) {
		this.transactTime = transactTime;
	}
	public long getFee() {
		return fee;
	}
	public void setFee(long fee) {
		this.fee = fee;
	}
	public int getLastCommission() {
		return lastCommission;
	}
	public void setLastCommission(int lastCommission) {
		this.lastCommission = lastCommission;
	}
	public int getCummCommission() {
		return cummCommission;
	}
	public void setCummCommission(int cummCommission) {
		this.cummCommission = cummCommission;
	}
	public String getTrdMatchID() {
		return trdMatchID;
	}
	public void setTrdMatchID(String trdMatchID) {
		this.trdMatchID = trdMatchID;
	}
	public String getOrdStatus() {
		return ordStatus;
	}
	public void setOrdStatus(String ordStatus) {
		this.ordStatus = ordStatus;
	}
	public long getOrigEscrow() {
		return origEscrow;
	}
	public void setOrigEscrow(long origEscrow) {
		this.origEscrow = origEscrow;
	}
	public long getLeavesEscrow() {
		return leavesEscrow;
	}
	public void setLeavesEscrow(long leavesEscrow) {
		this.leavesEscrow = leavesEscrow;
	}
	public int getPeggedPrice() {
		return peggedPrice;
	}
	public void setPeggedPrice(int peggedPrice) {
		this.peggedPrice = peggedPrice;
	}
	public String getLastLiquidityInd() {
		return lastLiquidityInd;
	}
	public void setLastLiquidityInd(String lastLiquidityInd) {
		this.lastLiquidityInd = lastLiquidityInd;
	}
	public String getSubmitTime() {
		return submitTime;
	}
	public void setSubmitTime(String submitTime) {
		this.submitTime = submitTime;
	}
	public String getOrdRejReason() {
		return ordRejReason;
	}
	public void setOrdRejReason(String ordRejReason) {
		this.ordRejReason = ordRejReason;
	}
	public String getCxlRejReason() {
		return cxlRejReason;
	}
	public void setCxlRejReason(String cxlRejReason) {
		this.cxlRejReason = cxlRejReason;
	}
	public boolean isEscrowRestricted() {
		return escrowRestricted;
	}
	public void setEscrowRestricted(boolean escrowRestricted) {
		this.escrowRestricted = escrowRestricted;
	}

	@Override
	public String getId() {
		return orderId;
	}

	@Override
	public boolean isNew() {
		return isNew;
	}
}
