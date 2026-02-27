export interface VacancyDto {
  id: string;
  userId: string;
  userName: string;
  serviceId: string;
  serviceName: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
  appointmentId?: string;
  createdAt: string;
}

export interface CreateVacancyRequest {
  userId: string;
  serviceId: string;
  startTime: string;
  endTime: string;
}

export interface BulkCreateVacancyRequest {
  userId: string;
  serviceId: string;
  slots: { startTime: string; endTime: string }[];
}
