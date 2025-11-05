import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CreateBusinessCardRequest, Gender } from '../../core/api.types';

@Component({
  selector: 'app-add',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add.component.html',
})
export class AddComponent {
  // نموذج الإدخال اليدوي
  name = signal<string>('');
  email = signal<string>('');
  phone = signal<string>('');
  address = signal<string>('');
  gender = signal<Gender>('Male');
  dateOfBirth = signal<string>(''); // yyyy-MM-dd
  photoBase64 = signal<string | null>(null);
  photoSizeBytes = signal<number | null>(null);

  // تبويب الاستيراد
  importTab = signal<'manual' | 'csv' | 'xml'>('manual');
  csvFile = signal<File | null>(null);
  xmlFile = signal<File | null>(null);

  // Drag over state
  isDragOver = signal<boolean>(false);

  constructor(private api: ApiService) {}

  // تحميل الصورة ومعاينتها
  onImageSelected(file: File | null) {
    if (!file) {
      this.photoBase64.set(null);
      this.photoSizeBytes.set(null);
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const b64 = (reader.result as string)?.split(',')[1] ?? '';
      this.photoBase64.set(b64);
      this.photoSizeBytes.set(file.size);
    };
    reader.readAsDataURL(file);
  }

  // Drag & Drop للصور
  onDragOver(evt: DragEvent) {
    evt.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(evt: DragEvent) {
    evt.preventDefault();
    this.isDragOver.set(false);
  }

  onDrop(evt: DragEvent) {
    evt.preventDefault();
    this.isDragOver.set(false);
    const file = evt.dataTransfer?.files?.[0];
    if (file) this.onImageSelected(file);
  }

  // حفظ يدوي
  save() {
    const payload: CreateBusinessCardRequest = {
      name: this.name(),
      gender: this.gender(),
      dateOfBirth: this.dateOfBirth() || null,
      email: this.email(),
      phone: this.phone(),
      address: this.address(),
      photoBase64: this.photoBase64(),
      photoSizeBytes: this.photoSizeBytes(),
    };

    this.api.create(payload).subscribe({
      next: (res) => {
        alert(res.message || 'Created');
        this.resetForm();
      },
      error: (err) => alert(err?.message || 'Error while saving'),
    });
  }

  resetForm() {
    this.name.set('');
    this.email.set('');
    this.phone.set('');
    this.address.set('');
    this.gender.set('Male');
    this.dateOfBirth.set('');
    this.photoBase64.set(null);
    this.photoSizeBytes.set(null);
  }

  // استيراد CSV/XML
  onCsvChosen(file: File | null) {
    this.csvFile.set(file);
  }

  onXmlChosen(file: File | null) {
    this.xmlFile.set(file);
  }

  uploadCsv() {
    const f = this.csvFile();
    if (!f) {
      alert('Please choose a CSV file first.');
      return;
    }
    this.api.importCsv(f).subscribe({
      next: (res) => alert(res.message || `Imported: ${res.data}`),
      error: (err) => alert(err?.message || 'Error uploading CSV'),
    });
  }

  uploadXml() {
    const f = this.xmlFile();
    if (!f) {
      alert('Please choose an XML file first.');
      return;
    }
    this.api.importXml(f).subscribe({
      next: (res) => alert(res.message || `Imported: ${res.data}`),
      error: (err) => alert(err?.message || 'Error uploading XML'),
    });
  }
}
