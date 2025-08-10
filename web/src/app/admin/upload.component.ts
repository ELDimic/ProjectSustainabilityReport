import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentsService } from '../core/documents.service';

@Component({
  selector: 'app-upload', standalone: true, imports: [CommonModule, FormsModule],
  template: `
  <h1>Carica PDF</h1>
  <form (ngSubmit)="submit()">
    <input type="file" (change)="onFile($event)" accept="application/pdf" required />
    <input [(ngModel)]="year" name="year" placeholder="Anno (es. 2024)"/>
    <input [(ngModel)]="summary" name="summary" placeholder="Descrizione"/>
    <button type="submit" [disabled]="!file">Carica</button>
  </form>
  <p *ngIf="ok">Caricato!</p>`
})
export class UploadComponent {
  private api = inject(DocumentsService);
  file?: File; year = ''; summary = ''; ok = false;
  onFile(e: Event) { const input = e.target as HTMLInputElement; this.file = input.files?.[0] || undefined; }
  submit() { if (!this.file) return; this.api.upload(this.file, this.year, this.summary).subscribe(() => this.ok = true); }
}
