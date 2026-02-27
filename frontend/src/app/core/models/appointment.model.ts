import { TransactionDto } from './transaction.model';

export interface AppointmentDto {
  id: string;
  customerId: string;
  customerName: string;
  userId: string;
  userName: string;
  serviceId: string;
  serviceName: string;
  vacancyId?: string;
  priceInCents: number;
  startTime: string;
  endTime: string;
  status: string;
  notes?: string;
  createdAt: string;
  transaction?: TransactionDto;
}

export interface CreateAppointmentRequest {
  customerId: string;
  userId: string;
  serviceId: string;
  vacancyId?: string;
  startTime: string;
  endTime: string;
  notes?: string;
}

export interface UpdateAppointmentRequest {
  startTime: string;
  endTime: string;
  notes?: string;
}
