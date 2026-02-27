export interface CustomerDto {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  createdAt: string;
}

export interface CreateCustomerRequest {
  fullName: string;
  email: string;
  phone?: string;
}

export interface UpdateCustomerRequest {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
}
