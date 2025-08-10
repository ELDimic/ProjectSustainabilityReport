import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export type LoginRes = { token: string; email: string; fullName?: string; functionalities: string[] };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = `${environment.apiBase}/auth`;

  login(email: string, password: string) {
    return this.http.post<LoginRes>(`${this.base}/login`, { email, password });
  }
  setToken(t: string) { localStorage.setItem('jwt', t); }
  get token() { return localStorage.getItem('jwt'); }
  logout() { localStorage.removeItem('jwt'); }
  has(func: string) {
    const payload = this.decode();
    return payload?.func?.includes(func) || payload?.func?.includes('admin');
  }
  private decode() {
    const t = this.token; if (!t) return null;
    const [, p] = t.split('.'); try { return JSON.parse(atob(p)); } catch { return null; }
  }
}
