import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key, i18n: { language: "en" } }),
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("@/features/auth", () => ({
  RegisterButton: () => <button data-testid="register-btn">Register</button>,
}));

vi.mock("@/features/chat", () => ({
  UnreadBadge: () => <span data-testid="unread-badge" />,
}));

vi.mock("../../LanguageSwitcher", () => ({
  default: () => <div data-testid="language-switcher" />,
}));

vi.mock("../../ui/button", () => ({
  Button: ({
    children,
    ...props
  }: {
    children: React.ReactNode;
    [key: string]: unknown;
  }) => <button {...props}>{children}</button>,
}));

vi.mock("./ThemeToggle", () => ({
  default: () => <div data-testid="theme-toggle" />,
}));

vi.mock("./UserMenu", () => ({
  default: () => <div data-testid="user-menu" />,
}));

import Header from "./Header";

describe("Header", () => {
  it("renders logo and navigation links", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: null, permissions: [] } }),
    );
    render(<Header />);

    expect(screen.getByRole("link", { name: /header.sellYourCar/i })).toBeInTheDocument();
    expect(screen.getByTestId("language-switcher")).toBeInTheDocument();
    expect(screen.getByTestId("theme-toggle")).toBeInTheDocument();
  });

  it("shows register button when user is not logged in", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: null, permissions: [] } }),
    );
    render(<Header />);

    expect(screen.getByTestId("register-btn")).toBeInTheDocument();
    expect(screen.queryByTestId("user-menu")).not.toBeInTheDocument();
  });

  it("shows user menu and inbox when user is logged in", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: "user-123", permissions: [] } }),
    );
    render(<Header />);

    expect(screen.getByTestId("user-menu")).toBeInTheDocument();
    expect(screen.getByText("header.inbox")).toBeInTheDocument();
    expect(screen.queryByTestId("register-btn")).not.toBeInTheDocument();
  });

  it("shows saved listings link when user is logged in", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { userId: "user-123", permissions: [] } }),
    );
    const { container } = render(<Header />);

    const savedAnchor = container.querySelector('a[href="/saved"]');
    expect(savedAnchor).toBeInTheDocument();
  });
});
