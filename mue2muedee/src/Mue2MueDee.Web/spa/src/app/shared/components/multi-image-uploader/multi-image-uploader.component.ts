import { Component, ElementRef, Input, ViewChild, forwardRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../../../core/models/api-response.model';

interface UploadResponse {
  url: string;
  fileName: string;
  size: number;
}

/**
 * อัปโหลดรูปหลายรูป (ไม่จำกัดจำนวน) — value เป็น string[] ลำดับแรก = รูปปก
 * รองรับ Drag & Drop ไฟล์หลายไฟล์, ลากเรียงลำดับรูป, ตั้งเป็นรูปปก, Lightbox
 */
@Component({
  selector: 'app-multi-image-uploader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './multi-image-uploader.component.html',
  styleUrl: './multi-image-uploader.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultiImageUploaderComponent),
      multi: true
    }
  ]
})
export class MultiImageUploaderComponent implements ControlValueAccessor {
  private readonly http = inject(HttpClient);

  @Input() label?: string;
  @Input() hint?: string;
  @Input() disabled = false;
  @Input() errorMessage?: string | null;

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  images: string[] = [];
  uploadingCount = 0;
  uploadError: string | null = null;
  isDragging = false;
  dragIndex: number | null = null;
  previewIndex: number | null = null;
  urlDraft = '';
  showUrlInput = false;

  private onChange: (val: string[]) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(val: unknown): void {
    this.images = Array.isArray(val) ? val.filter((v): v is string => typeof v === 'string' && !!v) : [];
  }
  registerOnChange(fn: (val: string[]) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean): void { this.disabled = isDisabled; }

  get isUploading(): boolean { return this.uploadingCount > 0; }
  get uploadingSlots(): number[] { return Array.from({ length: this.uploadingCount }, (_, i) => i); }

  triggerFileInput(): void {
    if (this.disabled) return;
    this.fileInput?.nativeElement.click();
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) this.uploadFiles(Array.from(input.files));
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.disabled && this.dragIndex === null) this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    if (this.disabled || this.dragIndex !== null) return;
    const files = event.dataTransfer?.files;
    if (files?.length) this.uploadFiles(Array.from(files));
  }

  private uploadFiles(files: File[]): void {
    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    const valid = files.filter(f => allowed.includes(f.type) && f.size <= 10 * 1024 * 1024);
    const rejected = files.length - valid.length;
    this.uploadError = rejected > 0 ? `ข้าม ${rejected} ไฟล์ (รองรับ JPG/PNG/WebP/GIF ขนาดไม่เกิน 10MB)` : null;

    // อัปโหลดทีละไฟล์แบบขนาน เพื่อให้รูปขึ้นทยอยและไม่ชน request size limit
    for (const file of valid) {
      this.uploadingCount++;
      const formData = new FormData();
      formData.append('file', file);
      this.http.post<ApiResponse<UploadResponse>>('/api/uploads/image', formData).subscribe({
        next: res => {
          this.uploadingCount--;
          if (res.data?.url) this.update([...this.images, res.data.url]);
        },
        error: err => {
          this.uploadingCount--;
          this.uploadError = err.error?.message || `อัปโหลด '${file.name}' ไม่สำเร็จ`;
        }
      });
    }
  }

  addUrl(): void {
    const url = this.urlDraft.trim();
    if (!url) return;
    if (!this.images.includes(url)) this.update([...this.images, url]);
    this.urlDraft = '';
  }

  remove(index: number, event?: MouseEvent): void {
    event?.stopPropagation();
    if (this.disabled) return;
    this.update(this.images.filter((_, i) => i !== index));
  }

  setCover(index: number, event?: MouseEvent): void {
    event?.stopPropagation();
    if (index === 0 || this.disabled) return;
    const next = [...this.images];
    const [img] = next.splice(index, 1);
    next.unshift(img);
    this.update(next);
  }

  // ---- ลากเรียงลำดับรูป ----
  onThumbDragStart(index: number): void { this.dragIndex = index; }

  onThumbDragOver(index: number, event: DragEvent): void {
    event.preventDefault();
    if (this.dragIndex === null || this.dragIndex === index) return;
    const next = [...this.images];
    const [img] = next.splice(this.dragIndex, 1);
    next.splice(index, 0, img);
    this.images = next;
    this.dragIndex = index;
  }

  onThumbDragEnd(): void {
    if (this.dragIndex !== null) this.update(this.images);
    this.dragIndex = null;
  }

  // ---- Lightbox ----
  openPreview(index: number): void { this.previewIndex = index; }
  closePreview(): void { this.previewIndex = null; }
  prev(event: MouseEvent): void {
    event.stopPropagation();
    if (this.previewIndex === null) return;
    this.previewIndex = (this.previewIndex - 1 + this.images.length) % this.images.length;
  }
  next(event: MouseEvent): void {
    event.stopPropagation();
    if (this.previewIndex === null) return;
    this.previewIndex = (this.previewIndex + 1) % this.images.length;
  }

  private update(next: string[]): void {
    this.images = next;
    this.onChange([...next]);
    this.onTouched();
  }
}
