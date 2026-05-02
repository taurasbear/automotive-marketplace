import type { ReactNode } from "react";

type DashboardTileProps = {
  icon: ReactNode;
  title: string;
  count: number;
  subtitle: string;
  detail: string;
  isHighlighted?: boolean;
  onClick?: () => void;
};

export function DashboardTile({
  icon,
  title,
  count,
  subtitle,
  detail,
  isHighlighted,
  onClick,
}: DashboardTileProps) {
  return (
    <button
      onClick={onClick}
      className={`bg-card text-card-foreground flex flex-col gap-1 rounded-lg border p-4 text-left transition-colors hover:bg-accent ${
        isHighlighted ? "border-primary/50 shadow-sm" : ""
      }`}
    >
      <div className="flex items-center justify-between">
        <span className="text-muted-foreground">{icon}</span>
        {count > 0 && (
          <span className="bg-primary text-primary-foreground rounded-full px-2 py-0.5 text-[10px] font-semibold">
            {count}
          </span>
        )}
      </div>
      <div className="text-sm font-semibold">{title}</div>
      <div className="text-muted-foreground text-xs">{subtitle}</div>
      {detail && (
        <div className="text-muted-foreground mt-1 truncate text-[10px]">{detail}</div>
      )}
    </button>
  );
}