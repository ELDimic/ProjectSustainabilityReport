import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface ReportItem { name: string; year?: string; size: number; lastModified?: string; summary?: string; downloadUrl: string; }

@Injectable({ providedIn: 'root' })
export class DocumentsService {
  private http = inject(HttpClient);
  private base = `${environment.apiBase}/documents`;
  list(from?: string, to?: string, minutes = 30) {
    const params: any = {}; if (from) params.from = from; if (to) params.to = to; params.minutes = minutes;
    return this.http.get<ReportItem[]>(this.base, { params });
  }
  upload(file: File, year?: string, summary?: string) {
    const fd = new FormData(); fd.append('file', file); if (year) fd.append('year', year); if (summary) fd.append('summary', summary);
    return this.http.post(this.base, fd);
  }
}
