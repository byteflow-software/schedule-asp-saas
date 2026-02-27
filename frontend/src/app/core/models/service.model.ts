export interface ServiceDto {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  priceInCents: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateServiceRequest {
  name: string;
  description?: string;
  durationMinutes: number;
  priceInCents: number;
}

export interface UpdateServiceRequest {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  priceInCents: number;
  isActive: boolean;
}
