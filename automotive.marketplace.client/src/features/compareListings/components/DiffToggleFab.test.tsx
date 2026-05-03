import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    onClick,
    variant,
    className,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    variant?: string;
    className?: string;
  }) => (
    <button onClick={onClick} data-variant={variant} className={className}>
      {children}
    </button>
  ),
}));

import { DiffToggleFab } from "./DiffToggleFab";

describe("DiffToggleFab", () => {
  it("renders with 'Diff Only' text when inactive", () => {
    render(<DiffToggleFab active={false} onToggle={vi.fn()} />);
    expect(screen.getByText("diffToggle.diffOnly")).toBeInTheDocument();
  });

  it("renders with 'Show All' text when active", () => {
    render(<DiffToggleFab active={true} onToggle={vi.fn()} />);
    expect(screen.getByText("diffToggle.showAll")).toBeInTheDocument();
  });

  it("uses 'default' variant when active", () => {
    render(<DiffToggleFab active={true} onToggle={vi.fn()} />);
    const btn = screen.getByRole("button");
    expect(btn).toHaveAttribute("data-variant", "default");
  });

  it("uses 'outline' variant when inactive", () => {
    render(<DiffToggleFab active={false} onToggle={vi.fn()} />);
    const btn = screen.getByRole("button");
    expect(btn).toHaveAttribute("data-variant", "outline");
  });

  it("calls onToggle when clicked", () => {
    const onToggle = vi.fn();
    render(<DiffToggleFab active={false} onToggle={onToggle} />);
    fireEvent.click(screen.getByRole("button"));
    expect(onToggle).toHaveBeenCalledTimes(1);
  });
});
