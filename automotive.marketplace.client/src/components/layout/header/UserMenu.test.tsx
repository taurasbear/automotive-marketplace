import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

const { mockUseAppSelector, mockDispatch, mockLogoutAsync, mockNavigate } =
  vi.hoisted(() => ({
    mockUseAppSelector: vi.fn(),
    mockDispatch: vi.fn(),
    mockLogoutAsync: vi.fn().mockResolvedValue(undefined),
    mockNavigate: vi.fn().mockResolvedValue(undefined),
  }));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => mockDispatch,
}));

vi.mock("@/features/auth", () => ({
  useLogoutUser: () => ({ mutateAsync: mockLogoutAsync }),
  clearCredentials: () => ({ type: "auth/clearCredentials" }),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../../ui/dropdown-menu", () => ({
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
    asChild?: boolean;
  }) => <div onClick={onClick}>{children}</div>,
  DropdownMenuLabel: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuSeparator: () => <hr />,
  DropdownMenuTrigger: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
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

import UserMenu from "./UserMenu";

describe("UserMenu", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders without crashing", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: [] } }),
    );
    render(<UserMenu />);
    expect(screen.getByText("common:userMenu.sectionMyListings")).toBeInTheDocument();
  });

  it("shows My Listings link", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: [] } }),
    );
    render(<UserMenu />);
    expect(screen.getByText("common:userMenu.myListings")).toBeInTheDocument();
  });

  it("shows admin section when user has ManageMakes permission", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: ["ManageMakes"] } }),
    );
    render(<UserMenu />);
    expect(screen.getByText("common:userMenu.sectionAdmin")).toBeInTheDocument();
    expect(screen.getByText("common:userMenu.makes")).toBeInTheDocument();
    expect(screen.getByText("common:userMenu.models")).toBeInTheDocument();
    expect(screen.getByText("common:userMenu.variants")).toBeInTheDocument();
  });

  it("hides admin section when user does not have ManageMakes permission", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: [] } }),
    );
    render(<UserMenu />);
    expect(screen.queryByText("common:userMenu.sectionAdmin")).not.toBeInTheDocument();
  });

  it("shows settings and logout options", () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: [] } }),
    );
    render(<UserMenu />);
    expect(screen.getByText("common:userMenu.profileSettings")).toBeInTheDocument();
    expect(screen.getByText("common:userMenu.logOut")).toBeInTheDocument();
  });

  it("calls logout and navigates on logout click", async () => {
    mockUseAppSelector.mockImplementation((selector: (s: unknown) => unknown) =>
      selector({ auth: { permissions: [] } }),
    );
    render(<UserMenu />);

    const logoutButton = screen.getByText("common:userMenu.logOut");
    fireEvent.click(logoutButton);

    await vi.waitFor(() => {
      expect(mockLogoutAsync).toHaveBeenCalled();
    });
  });
});
