export interface TransactionDto {
  id: string;
  appointmentId: string;
  customerId: string;
  customerName: string;
  referenceNumber: string;
  amountInCents: number;
  status: string;
  type: string;
  description?: string;
  paidAt?: string;
  paymentMethod?: string;
  invoiceUrl?: string;
  pixQrCodeUrl?: string;
  createdAt: string;
}

export interface TransactionSummaryDto {
  totalRevenueCents: number;
  totalPendingCents: number;
  totalPaidCents: number;
  count: number;
}
