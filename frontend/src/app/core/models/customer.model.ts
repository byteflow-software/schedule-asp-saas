export interface CustomerDto {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  cpfCnpj: string;
  createdAt: string;
}

export interface CreateCustomerRequest {
  fullName: string;
  email: string;
  phone?: string;
  cpfCnpj: string;
}

export interface UpdateCustomerRequest {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  cpfCnpj: string;
}
