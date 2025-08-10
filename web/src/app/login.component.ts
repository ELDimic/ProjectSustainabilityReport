import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule],
  template: `
  <h1>Accedi</h1>
  <form (ngSubmit)="submit()">
    <input [(ngModel)]="email" name="email" type="email" placeholder="email" required />
    <input [(ngModel)]="password" name="password" type="password" placeholder="password" required />
    <button type="submit">Login</button>
  </form>`
})
export class LoginComponent {
  private auth = inject(AuthService); private router = inject(Router);
  email = ''; password = '';
  submit() {
    this.auth.login(this.email, this.password).subscribe(res => { this.auth.setToken(res.token); this.router.navigateByUrl('/'); });
  }
}
