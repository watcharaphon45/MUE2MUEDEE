import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SaleService } from '../sale.service';
import { StatusBadgeComponent, TextareaComponent } from '../../../shared/components';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    StatusBadgeComponent,
    TextareaComponent
  ],
  templateUrl: './sale-detail.component.html',
  styleUrl: './sale-detail.component.css'
})
export class SaleDetailComponent implements OnInit {
  protected readonly saleService = inject(SaleService);
  private readonly route = inject(ActivatedRoute);

  saleId = signal<string>('');
  showCancelModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  feedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);

  cancelForm = new FormGroup({
    reason: new FormControl('', [Validators.required])
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.saleId.set(id);
      this.loadDetails(id);
    }
  }

  loadDetails(id: string): void {
    this.saleService.getSaleById(id).subscribe();
  }

  printReceipt(): void {
    window.print();
  }

  openCancel(): void {
    this.cancelForm.reset();
    this.showCancelModal.set(true);
  }

  closeCancel(): void {
    this.showCancelModal.set(false);
  }

  submitCancel(): void {
    if (this.cancelForm.invalid) {
      this.cancelForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const reason = this.cancelForm.controls.reason.value!;

    this.saleService.cancelSale(this.saleId(), reason).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closeCancel();
        this.feedback.set({ type: 'success', text: 'ยกเลิกรายการขายและนำสินค้ากลับเข้าคลังเรียบร้อยแล้ว' });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการยกเลิกบิลขาย' });
      }
    });
  }
}
