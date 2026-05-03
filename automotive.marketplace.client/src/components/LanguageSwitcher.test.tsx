import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

const { mockChangeLanguage } = vi.hoisted(() => ({
  mockChangeLanguage: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "lt", changeLanguage: mockChangeLanguage },
  }),
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

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    ...props
  }: {
    children: React.ReactNode;
    [key: string]: unknown;
  }) => <button {...props}>{children}</button>,
}));

import LanguageSwitcher from "./LanguageSwitcher";

describe("LanguageSwitcher", () => {
  it("renders current language label", () => {
    render(<LanguageSwitcher />);
    const allLT = screen.getAllByText("LT");
    expect(allLT.length).toBeGreaterThanOrEqual(1);
  });

  it("renders all language options", () => {
    render(<LanguageSwitcher />);
    const items = screen.getAllByRole("menuitem");
    expect(items).toHaveLength(2);
    expect(items[0]).toHaveTextContent("LT");
    expect(items[1]).toHaveTextContent("EN");
  });

  it("calls changeLanguage with 'en' when EN is clicked", () => {
    render(<LanguageSwitcher />);
    const enItem = screen.getAllByRole("menuitem")[1];
    fireEvent.click(enItem);
    expect(mockChangeLanguage).toHaveBeenCalledWith("en");
  });

  it("calls changeLanguage with 'lt' when LT is clicked", () => {
    render(<LanguageSwitcher />);
    const ltItem = screen.getAllByRole("menuitem")[0];
    fireEvent.click(ltItem);
    expect(mockChangeLanguage).toHaveBeenCalledWith("lt");
  });
});
