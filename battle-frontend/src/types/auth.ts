export interface LoginDTO {
  Account: string;
  Password: string;
}

export interface LoginResponse {
  token: string;
  id: string;
  profession: string | null;
  name: string;
}