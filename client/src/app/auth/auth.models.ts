export interface UsuarioDto {
  id: string;
  userName: string;
  email: string;
}

export interface TokenResponseRaw {
  Chave?: string;
  chave?: string;
  DataExpiracao?: string;
  dataExpiracao?: string;
  Usuario?: UsuarioDto;
  usuario?: UsuarioDto;
}

export interface TokenResponse {
  chave: string;
  dataExpiracao: string;
  usuario: UsuarioDto;
}
