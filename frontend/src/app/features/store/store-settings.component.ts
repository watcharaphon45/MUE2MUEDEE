import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { StoreService } from './store.service';
import { StoreDetails, StoreUser } from './store.models';
import { AuthService } from '../../core/auth/auth.service';
import { 
  TextboxComponent, 
  TextareaComponent, 
  SelectComponent, 
  SelectOption, 
  StatusBadgeComponent 
} from '../../shared/components';

@Component({
  selector: 'app-store-settings',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    TextboxComponent,
    TextareaComponent,
    SelectComponent,
    StatusBadgeComponent
  ],
  templateUrl: './store-settings.component.html',
  styleUrl: './store-settings.component.css'
})
export class StoreSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  protected readonly storeService = inject(StoreService);
  protected readonly authService = inject(AuthService);

  readonly activeTab = signal<'profile' | 'users'>('profile');
  readonly storeCode = signal<string>('');
  readonly usersList = signal<StoreUser[]>([]);
  readonly saving = signal(false);
  readonly saveSuccess = signal(false);
  readonly showAddUserModal = signal(false);

  readonly roleOptions: SelectOption[] = [
    { value: 'Staff', label: 'Staff (พนักงานหน้าร้าน — ไม่เห็นต้นทุน/กำไร)' },
    { value: 'Admin', label: 'Admin (ผู้จัดการร้าน)' }
  ];

  readonly storeForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    phone: [''],
    email: ['', [Validators.email]],
    address: [''],
    taxId: [''],
    primaryColor: ['#4f46e5']
  });

  readonly addUserForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    fullName: ['', [Validators.required]],
    phoneNumber: [''],
    temporaryPassword: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Staff', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadStore();
    this.loadUsers();
  }

  getStoreErrorMessage(fieldName: string): string | null {
    const field = this.storeForm.get(fieldName);
    if (field && field.invalid && (field.dirty || field.touched)) {
      if (fieldName === 'name') return 'กรุณาระบุชื่อร้านค้า';
      if (fieldName === 'email') return 'รูปแบบอีเมลไม่ถูกต้อง';
    }
    return null;
  }

  getUserErrorMessage(fieldName: string): string | null {
    const field = this.addUserForm.get(fieldName);
    if (field && field.invalid && (field.dirty || field.touched)) {
      if (fieldName === 'email') return 'กรุณากรอกอีเมลที่ถูกต้อง';
      if (fieldName === 'fullName') return 'กรุณากรอกชื่อ-นามสกุล';
      if (fieldName === 'temporaryPassword') return 'รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร';
    }
    return null;
  }

  loadStore(): void {
    this.storeService.getCurrentStore().subscribe({
      next: (data: StoreDetails) => {
        this.storeCode.set(data.code);
        this.storeForm.patchValue({
          name: data.name,
          phone: data.phone || '',
          email: data.email || '',
          address: data.address || '',
          taxId: data.taxId || '',
          primaryColor: data.primaryColor || '#4f46e5'
        });

        if (!this.authService.isAdmin()) {
          this.storeForm.disable();
        }
      },
      error: () => {}
    });
  }

  loadUsers(): void {
    this.storeService.getStoreUsers().subscribe({
      next: (list: StoreUser[]) => this.usersList.set(list),
      error: () => {}
    });
  }

  onSaveStore(): void {
    if (this.storeForm.invalid) {
      this.storeForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveSuccess.set(false);

    this.storeService.updateCurrentStore(this.storeForm.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 3000);
      },
      error: () => this.saving.set(false)
    });
  }

  onAddUserSubmit(): void {
    if (this.addUserForm.invalid) {
      this.addUserForm.markAllAsTouched();
      return;
    }

    this.storeService.addUserToStore(this.addUserForm.value).subscribe({
      next: () => {
        this.showAddUserModal.set(false);
        this.loadUsers();
        this.addUserForm.reset({ role: 'Staff' });
      },
      error: () => {}
    });
  }
}
