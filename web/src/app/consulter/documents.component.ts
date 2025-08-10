import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentsService, ReportItem } from '../core/documents.service';

@Component({
  selector: 'app-documents', standalone: true, imports: [CommonModule, FormsModule],
  template: `
  <section>
    <h1>Documenti di Sostenibilità</h1>
    <form (ngSubmit)="load()" class="toolbar">
      <label>Dal <input type="date" [(ngModel)]="from" name="from"></label>
      <label>Al <input type="date" [(ngModel)]="to" name="to"></label>
      <label>Validità link (min) <input type="number" [(ngModel)]="minutes" name="minutes" min="5" max="120"></label>
      <button type="submit">Cerca</button>
    </form>
    <ul *ngIf="items().length; else empty">
      <li *ngFor="let r of items()">
        <strong>{{ r.year || 'N/D' }}</strong> — {{ r.summary || r.name }}
        <a [href]="r.downloadUrl">Scarica</a>
      </li>
    </ul>
    <ng-template #empty><p>Nessun documento nel periodo selezionato.</p></ng-template>
  </section>`
})
export class DocumentsComponent {
  private api = inject(DocumentsService);
  items = signal<ReportItem[]>([]);
  from = ''; to = ''; minutes = 30;
  ngOnInit() { this.load(); }
  load() { this.api.list(this.from || undefined, this.to || undefined, this.minutes).subscribe(x => this.items.set(x)); }
}
