import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly pendingCount = signal(0);
  readonly pendingRequests = this.pendingCount.asReadonly();
  readonly isLoading = computed(() => this.pendingCount() > 0);

  begin(): () => void {
    this.pendingCount.update(count => count + 1);
    let finished = false;
    return () => {
      if (finished) return;
      finished = true;
      this.pendingCount.update(count => count - 1);
    };
  }
}
