export interface Client {
  id: string;
  name: string;
  email: string;
  phone: string;
  status: 'active' | 'inactive' | 'archived';
}

export interface ClientDto {
  name: string;
  email: string;
  phone: string;
  status: 'active' | 'inactive' | 'archived';
}
