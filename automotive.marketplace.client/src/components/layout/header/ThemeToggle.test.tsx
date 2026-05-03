import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

const { mockSetTheme } = vi.hoisted(() => ({
  mockSetTheme: vi.fn(),
}));

vi.mock("@/components/providers/ThemeProvider", () => ({
  useTheme: () => ({ setTheme: mockSetTheme, theme: "light" }),
}));

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    ...props
  }: {
    children: React.ReactNode;
    [key: string]: unknown;
  }) => <button {...props}>{children}</button>,
}));

vi.mock("@/components/ui/dropdown-menu", () => ({
  DropdownMenu: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuItem: ({
    children,
    onClick,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
  }) => (
    <button onClick={onClick} role="menuitem">
      {children}
    </button>
  ),
  DropdownMenuTrigger: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
}));

import ThemeToggle from "./ThemeToggle";

describe("ThemeToggle", () => {
  it("renders toggle button with screen reader text", () => {
    render(<ThemeToggle />);
    expect(screen.getByText("Toggle theme")).toBeInTheDocument();
  });

  it("renders Light, Dark, and System options", () => {
    render(<ThemeToggle />);
    expect(screen.getByText("Light")).toBeInTheDocument();
    expect(screen.getByText("Dark")).toBeInTheDocument();
    expect(screen.getByText("System")).toBeInTheDocument();
  });

  it("calls setTheme with 'light' when Light is clicked", () => {
    render(<ThemeToggle />);
    fireEvent.click(screen.getByText("Light"));
    expect(mockSetTheme).toHaveBeenCalledWith("light");
  });

  it("calls setTheme with 'dark' when Dark is clicked", () => {
    render(<ThemeToggle />);
    fireEvent.click(screen.getByText("Dark"));
    expect(mockSetTheme).toHaveBeenCalledWith("dark");
  });

  it("calls setTheme with 'system' when System is clicked", () => {
    render(<ThemeToggle />);
    fireEvent.click(screen.getByText("System"));
    expect(mockSetTheme).toHaveBeenCalledWith("system");
  });
});
