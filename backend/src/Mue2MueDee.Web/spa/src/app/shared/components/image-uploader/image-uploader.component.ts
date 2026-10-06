import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  ViewChild,
  forwardRef,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../../../core/models/api-response.model';

interface UploadResponse {
  url: string;
  fileName: string;
  size: number;
}

@Component({
  selector: 'app-image-uploader',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './image-uploader.component.html',
  styleUrl: './image-uploader.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ImageUploaderComponent),
      multi: true
    }
  ]
})
export class ImageUploaderComponent implements ControlValueAccessor {
  private readonly http = inject(HttpClient);

  @Input() label?: string;
  @Input() placeholder: string = 'วาง URL รูปภาพ หรือคลิกเพื่ออัปโหลด';
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;
  @Input() hint?: string;
  @Input() errorMessage?: string | null;

  @Output() imageSelected = new EventEmitter<string>();

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  value: string = '';
  isUploading: boolean = false;
  uploadError: string | null = null;
  isDragging: boolean = false;
  showUrlInput: boolean = false;

  // Fullscreen Preview Modal
  showPreviewModal: boolean = false;

  onChange: (val: string) => void = () => {};
  onTouched: () => void = () => {};

  // ControlValueAccessor
  writeValue(val: any): void {
    this.value = typeof val === 'string' ? val : '';
    this.uploadError = null;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  get hasImage(): boolean {
    return !!this.value && this.value.trim().length > 0;
  }

  triggerFileInput(): void {
    if (this.disabled || this.isUploading) return;
    this.fileInput?.nativeElement.click();
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.disabled) {
      this.isDragging = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    if (this.disabled) return;

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.uploadFile(event.dataTransfer.files[0]);
    }
  }

  private uploadFile(file: File): void {
    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!allowed.includes(file.type)) {
      this.uploadError = 'รองรับเฉพาะไฟล์รูปภาพ (JPG, PNG, WebP, GIF)';
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.uploadError = 'ขนาดไฟล์ต้องไม่เกิน 10MB';
      return;
    }

    this.isUploading = true;
    this.uploadError = null;

    const formData = new FormData();
    formData.append('file', file);

    this.http.post<ApiResponse<UploadResponse>>('/api/uploads/image', formData).subscribe({
      next: res => {
        this.isUploading = false;
        if (res.data?.url) {
          this.setValue(res.data.url);
        }
      },
      error: err => {
        this.isUploading = false;
        this.uploadError = err.error?.message || 'อัปโหลดรูปภาพไม่สำเร็จ กรุณาลองใหม่อีกครั้ง';
      }
    });
  }

  onUrlInputChange(newUrl: string): void {
    this.setValue(newUrl.trim());
  }

  setValue(val: string): void {
    this.value = val;
    this.onChange(this.value);
    this.imageSelected.emit(this.value);
    this.onTouched();
    this.uploadError = null;
  }

  clearImage(event?: MouseEvent): void {
    if (event) event.stopPropagation();
    if (this.disabled) return;
    this.setValue('');
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  openPreview(event?: MouseEvent): void {
    if (event) event.stopPropagation();
    if (this.hasImage) {
      this.showPreviewModal = true;
    }
  }

  closePreview(): void {
    this.showPreviewModal = false;
  }

  toggleUrlInput(): void {
    this.showUrlInput = !this.showUrlInput;
  }
}
