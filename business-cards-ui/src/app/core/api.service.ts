import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  ApiResponse,
  Pagination,
  BusinessCardDto,
  BusinessCardParams,
  CreateBusinessCardRequest,
} from './api.types';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = '/api/BusinessCards'; 

  constructor(private http: HttpClient) {}

  getAll(params: Partial<BusinessCardParams>): Observable<ApiResponse<Pagination<BusinessCardDto>>> {
    let hp = new HttpParams()
      .set('pageNumber', String(params.pageNumber ?? 1))
      .set('pageSize', String(params.pageSize ?? 10));

    if (params.name) hp = hp.set('name', params.name);
    if (params.email) hp = hp.set('email', params.email);
    if (params.phone) hp = hp.set('phone', params.phone);
    if (params.gender) hp = hp.set('gender', params.gender);
    if (params.dateOfBirth) hp = hp.set('dateOfBirth', params.dateOfBirth);

    return this.http.get<ApiResponse<Pagination<BusinessCardDto>>>(this.baseUrl, { params: hp });
  }

  create(model: CreateBusinessCardRequest): Observable<ApiResponse<BusinessCardDto>> {
    return this.http.post<ApiResponse<BusinessCardDto>>(this.baseUrl, model);
  }

  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  importCsv(file: File): Observable<ApiResponse<number>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/import/csv`, fd);
  }

  importXml(file: File): Observable<ApiResponse<number>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/import/xml`, fd);
  }

  exportCsv(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export/csv`, { responseType: 'blob' });
  }

  exportXml(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export/xml`, { responseType: 'blob' });
  }

  downloadBlob(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    setTimeout(() => {
      URL.revokeObjectURL(url);
      a.remove();
    }, 0);
  }
}
