export interface Client {
  id: string;
  name: string;
  email: string;
  phone: string;
  status: 'active' | 'inactive';
}

export interface ClientDto {
  name: string;
  email: string;
  phone: string;
  status: 'active' | 'inactive';
}
