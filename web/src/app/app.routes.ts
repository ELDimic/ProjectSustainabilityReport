import { Routes } from '@angular/router';
import { need } from './core/func.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./consulter/documents.component').then(m => m.DocumentsComponent), canActivate: [need('download')] },
  { path: 'admin', loadComponent: () => import('./admin/upload.component').then(m => m.UploadComponent), canActivate: [need('upload')] },
  { path: 'login', loadComponent: () => import('./login.component').then(m => m.LoginComponent) },
  { path: '**', redirectTo: '' }
];
