import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { BusinessCardDto, Gender } from '../../core/api.types';

@Component({
  selector: 'app-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './list.component.html',
})
export class ListComponent {
  name = signal<string>('');
  email = signal<string>('');
  phone = signal<string>('');
  gender = signal<Gender | ''>('');
  dateOfBirth = signal<string>(''); 

  items = signal<BusinessCardDto[]>([]);
  pageNumber = signal<number>(1);
  pageSize = signal<number>(10);
  totalCount = signal<number>(0);
  loading = signal<boolean>(false);

  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize()))
  );

  constructor(private api: ApiService) {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.api.getAll({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      name: this.name() || undefined,
      email: this.email() || undefined,
      phone: this.phone() || undefined,
      gender: this.gender() || undefined,
      dateOfBirth: this.dateOfBirth() || undefined,
    }).subscribe({
      next: (res) => {
        this.items.set(res.data.data);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goFirstPageAndLoad() {
    this.pageNumber.set(1);
    this.load();
  }
genderText(g: any): string {
  if (g === 1 || g === 'Female') return 'Female';
  return 'Male';
}

dateOnly(d: string | null | undefined): string {
  if (!d) return '—';
  return d.slice(0, 10);
}
  resetFilters() {
    this.name.set('');
    this.email.set('');
    this.phone.set('');
    this.gender.set('');
    this.dateOfBirth.set('');
    this.goFirstPageAndLoad();
  }

  exportCsv() {
    this.api.exportCsv().subscribe(blob =>
      this.api.downloadBlob(
        blob,
        `business_cards_${new Date().toISOString().slice(0,19).replace(/[:T]/g,'-')}.csv`
      )
    );
  }

  exportXml() {
    this.api.exportXml().subscribe(blob =>
      this.api.downloadBlob(
        blob,
        `business_cards_${new Date().toISOString().slice(0,19).replace(/[:T]/g,'-')}.xml`
      )
    );
  }

  deleteItem(id: string) {
    if (!confirm('Are you sure?')) return;
    this.api.delete(id).subscribe(_ => this.load());
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.set(this.pageNumber() + 1);
      this.load();
    }
  }

  prevPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.set(this.pageNumber() - 1);
      this.load();
    }
  }
}
