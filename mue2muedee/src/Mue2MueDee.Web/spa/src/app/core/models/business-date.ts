const bangkokOffsetMs = 7 * 60 * 60 * 1000;

export function businessDateInput(date: Date): string {
  return new Date(date.getTime() + bangkokOffsetMs).toISOString().slice(0, 10);
}

export function apiDateBoundary(value: string, endOfDay = false): string {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
  return `${value}T${endOfDay ? '23:59:59.999' : '00:00:00.000'}+07:00`;
}
