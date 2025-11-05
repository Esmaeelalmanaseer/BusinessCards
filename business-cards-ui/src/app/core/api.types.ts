export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface Pagination<T> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  data: T[];
}

export type Gender = 'Male' | 'Female';

export interface BusinessCardDto {
  id: string;
  name: string;
  gender: Gender;
  dateOfBirth?: string | null; 
  email: string;
  phone: string;
  address: string;
  photoBase64?: string | null;
  photoSizeBytes?: number | null;
}

export interface CreateBusinessCardRequest {
  name: string;
  gender: Gender;
  dateOfBirth?: string | null;
  email: string;
  phone: string;
  address: string;
  photoBase64?: string | null;
  photoSizeBytes?: number | null;
}

export interface BusinessCardParams {
  pageNumber: number;
  pageSize: number;
  name?: string;
  email?: string;
  phone?: string;
  gender?: Gender | '';
  dateOfBirth?: string; 
}
