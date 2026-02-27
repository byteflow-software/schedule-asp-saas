export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantName: string;
  fullName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  tenantId: string;
  fullName: string;
  role: string;
  tokens: TokenPair;
}

export interface RegisterResponse {
  tenantId: string;
  userId: string;
  tokens: TokenPair;
}

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  token: string;
}

export interface DecodedToken {
  sub: string;
  email: string;
  tenant_id: string;
  role: string;
  exp: number;
}
