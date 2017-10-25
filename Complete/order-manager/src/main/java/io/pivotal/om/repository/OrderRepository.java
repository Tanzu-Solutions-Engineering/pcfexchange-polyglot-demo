package io.pivotal.om.repository;
import io.pivotal.om.domain.ExecutionReport;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface OrderRepository extends JpaRepository<ExecutionReport, String> {

    @Query("SELECT new io.pivotal.om.domain.ExecutionReport(c.execType, c.execId, c.tradeId, c.execRefID, c. totalNumReports, c.clOrdID, c.ordStatusReqID, c.origClOrdID, c.clientID, c.massStatusReqID, c.orderId, c. secondaryOrderId, c.symbol, c. price, c. stopPx, c. orderQty, c.ordType, c.side, c.timeInForce, c.pegScope, c.pegPriceType, c. pegOffset, c. seqNum, c.triggerPriceType, c. lastPx, c. lastQty, c. cumQty, c. avgPx, c. leavesQty, c.transactTime, c. fee, c. lastCommission, c. cummCommission, c.trdMatchID, c.ordStatus, c. origEscrow, c. leavesEscrow, c. peggedPrice, c.lastLiquidityInd, c.submitTime, c.ordRejReason, c.cxlRejReason,  escrowRestricted) FROM ExecutionReport c WHERE c.clientID = :clientId")
    public List<ExecutionReport> ordersByClient(@Param("clientId") String clientId);


}
