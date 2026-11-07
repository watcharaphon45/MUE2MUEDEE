import { TablePaginationComponent } from '../../../shared/components/table-pagination/table-pagination.component';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { SuperAdminService } from '../super-admin.service';
import { SuperAdminStoreItem } from '../super-admin.models';
import { TextboxComponent } from '../../../shared/components';

@Component({
  selector: 'app-super-admin-stores',
  standalone: true,
  imports: [TablePaginationComponent, CommonModule, ReactiveFormsModule, TextboxComponent],
  templateUrl: './super-admin-stores.component.html',
  styleUrl: './super-admin-stores.component.css'
})
export class SuperAdminStoresComponent implements OnInit {
  protected readonly adminService = inject(SuperAdminService);
  private readonly fb = inject(FormBuilder);

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly searchQuery = toSignal(this.searchControl.valueChanges, { initialValue: '' });
  readonly isModalOpen = signal<boolean>(false);
  readonly isSubmitting = signal<boolean>(false);
  readonly formError = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly statusError = signal<string | null>(null);
  readonly pendingStoreIds = signal<ReadonlySet<string>>(new Set());

  readonly storeForm = this.fb.nonNullable.group({
    storeName: ['', [Validators.required, Validators.minLength(2)]],
    storeCode: [''],
    phone: [''],
    email: ['', [Validators.email]],
    address: [''],
    primaryColor: ['#0284c7'],
    ownerFullName: ['', [Validators.required, Validators.minLength(2)]],
    ownerUsername: ['', [Validators.required, Validators.minLength(3)]],
    ownerEmail: ['', [Validators.required, Validators.email]],
    ownerPassword: ['', [Validators.required, Validators.minLength(6)]],
    ownerPhoneNumber: ['']
  });

  readonly filteredStores = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const list = this.adminService.stores();
    if (!q) return list;

    return list.filter(s =>
      s.name.toLowerCase().includes(q) ||
      s.code.toLowerCase().includes(q) ||
      (s.ownerFullName && s.ownerFullName.toLowerCase().includes(q)) ||
      (s.ownerUsername && s.ownerUsername.toLowerCase().includes(q)) ||
      (s.ownerEmail && s.ownerEmail.toLowerCase().includes(q)) ||
      (s.phone && s.phone.includes(q)) ||
      (s.email && s.email.toLowerCase().includes(q)) ||
      (s.ownerPhoneNumber && s.ownerPhoneNumber.includes(q))
    );
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.adminService.loadStores().subscribe({ error: () => {} });
    this.adminService.loadStats().subscribe({ error: () => {} });
  }

  openCreateModal(): void {
    this.storeForm.reset({
      storeName: '',
      storeCode: '',
      phone: '',
      email: '',
      address: '',
      primaryColor: '#0284c7',
      ownerFullName: '',
      ownerUsername: '',
      ownerEmail: '',
      ownerPassword: '',
      ownerPhoneNumber: ''
    });
    this.formError.set(null);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    if (this.isSubmitting()) return;
    this.isModalOpen.set(false);
    this.storeForm.controls.ownerPassword.reset('');
    this.formError.set(null);
  }

  onSubmit(): void {
    if (this.isSubmitting()) return;
    if (this.storeForm.invalid) {
      this.storeForm.markAllAsTouched();
      this.formError.set('กรุณากรอกข้อมูลที่จำเป็น (*) ให้ครบถ้วนและถูกต้อง');
      return;
    }

    this.isSubmitting.set(true);
    this.formError.set(null);

    const formVal = this.storeForm.getRawValue();

    this.adminService.createStoreWithOwner(formVal).subscribe({
      next: (created) => {
        this.isSubmitting.set(false);
        this.closeModal();
        this.showSuccess(`สร้างร้านค้า '${created.name}' และบัญชีเจ้าของร้าน '${created.ownerUsername}' สำเร็จแล้ว`);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.formError.set(err?.error?.message ?? 'เกิดข้อผิดพลาดในการสร้างร้านค้าและบัญชีผู้ใช้');
      }
    });
  }

  toggleStatus(store: SuperAdminStoreItem): void {
    if (this.pendingStoreIds().has(store.id)) return;
    const confirmMsg = store.isActive
      ? `คุณต้องการระงับการใช้งานร้านค้า '${store.name}' ใช่หรือไม่?`
      : `คุณต้องการเปิดการใช้งานร้านค้า '${store.name}' ใช่หรือไม่?`;

    if (!confirm(confirmMsg)) return;

    this.statusError.set(null);
    this.pendingStoreIds.update(ids => new Set([...ids, store.id]));
    this.adminService.toggleStoreStatus(store.id).pipe(finalize(() => {
      this.pendingStoreIds.update(ids => {
        const next = new Set(ids);
        next.delete(store.id);
        return next;
      });
    })).subscribe({
      next: (newStatus) => {
        this.showSuccess(newStatus ? `เปิดใช้งานร้าน '${store.name}' แล้ว` : `ระงับการใช้งานร้าน '${store.name}' แล้ว`);
      },
      error: (err) => {
        this.statusError.set(err?.error?.message ?? 'เกิดข้อผิดพลาดในการเปลี่ยนสถานะร้านค้า');
      }
    });
  }

  private showSuccess(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 4500);
  }
}
