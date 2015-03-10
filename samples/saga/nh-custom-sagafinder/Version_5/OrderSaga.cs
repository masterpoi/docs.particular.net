﻿using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Saga;
using System;

#region TheSagaNHibernate
public class OrderSaga : Saga<OrderSagaData>,
	IAmStartedByMessages<StartOrder>,
	IHandleMessages<PaymentTransactionCompleted>,
	IHandleMessages<CompleteOrder>
{
	static ILog logger = LogManager.GetLogger( typeof( OrderSaga ) );

	protected override void ConfigureHowToFindSaga( SagaPropertyMapper<OrderSagaData> mapper )
	{
		//NOP
	}


	public void Handle( StartOrder message )
	{
		Data.OrderId = message.OrderId;
		Data.PaymentTransactionId = Guid.NewGuid().ToString();

		logger.Info( string.Format( "Saga with OrderId {0} received StartOrder with OrderId {1}", Data.OrderId, message.OrderId ) );
		Bus.SendLocal( new IssuePaymentRequest
		{
			PaymentTransactionId = Data.PaymentTransactionId
		} );
	}

	public void Handle( PaymentTransactionCompleted message )
	{
		logger.Info( string.Format( "Transaction with Id {0} completed for order id {1}", Data.PaymentTransactionId, Data.OrderId ) );
		Bus.SendLocal( new CompleteOrder
		{
			OrderId = Data.OrderId
		} );
	}

	public void Handle( CompleteOrder message )
	{
		logger.Info( string.Format( "Saga with OrderId {0} received CompleteOrder with OrderId {1}", Data.OrderId, message.OrderId ) );
		MarkAsComplete();
	}
}
#endregion