export function getTimezoneOffsetLabel(): string {
  const offsetMinutes = new Date().getTimezoneOffset();
  if (offsetMinutes === 0) return "UTC+0";

  const sign = offsetMinutes <= 0 ? "+" : "-";
  const absMinutes = Math.abs(offsetMinutes);
  const hours = Math.floor(absMinutes / 60);
  const minutes = absMinutes % 60;

  if (minutes === 0) return `UTC${sign}${hours}`;
  return `UTC${sign}${hours}:${String(minutes).padStart(2, "0")}`;
}
