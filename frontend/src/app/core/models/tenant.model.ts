export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  cpfCnpj?: string;
  phone?: string;
  email?: string;
  address?: string;
  addressNumber?: string;
  complement?: string;
  neighborhood?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  logoUrl?: string;
  asaasWalletId?: string;
  hasAsaasIntegration: boolean;
  createdAt: string;
}

export interface UpdateTenantRequest {
  name: string;
  cpfCnpj?: string;
  phone?: string;
  email?: string;
  address?: string;
  addressNumber?: string;
  complement?: string;
  neighborhood?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  logoUrl?: string;
}
