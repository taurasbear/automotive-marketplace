import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import CreateListing from "./CreateListing";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => vi.fn(),
}));

vi.mock("@/features/auth", () => ({
  selectAccessToken: "selectAccessToken",
}));

vi.mock("@/features/createListing", () => ({
  CreateListingForm: ({
    submitDisabled,
    className,
  }: {
    submitDisabled?: boolean;
    className?: string;
  }) => (
    <div data-testid="create-listing-form" data-disabled={submitDisabled} className={className}>
      {submitDisabled ? "form-disabled" : "form-enabled"}
    </div>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
  Trans: ({ i18nKey }: { i18nKey: string }) => <span>{i18nKey}</span>,
}));

vi.mock("@tanstack/react-router", () => ({
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

describe("CreateListing page", () => {
  beforeEach(() => {
    mockUseAppSelector.mockReset();
  });

  it("renders the page title", () => {
    mockUseAppSelector.mockReturnValue("some-token");
    render(<CreateListing />);
    expect(screen.getByText("createPage.title")).toBeInTheDocument();
  });

  it("shows guest warning when not authenticated", () => {
    mockUseAppSelector.mockReturnValue(null);
    render(<CreateListing />);

    expect(screen.getByRole("alert")).toBeInTheDocument();
    expect(screen.getByText("listings:createPage.guestWarning")).toBeInTheDocument();
  });

  it("disables form for guest users", () => {
    mockUseAppSelector.mockReturnValue(null);
    render(<CreateListing />);

    const form = screen.getByTestId("create-listing-form");
    expect(form).toHaveAttribute("data-disabled", "true");
    expect(form).toHaveTextContent("form-disabled");
  });

  it("does not show guest warning when authenticated", () => {
    mockUseAppSelector.mockReturnValue("valid-access-token");
    render(<CreateListing />);

    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("renders form enabled for authenticated users", () => {
    mockUseAppSelector.mockReturnValue("valid-access-token");
    render(<CreateListing />);

    const form = screen.getByTestId("create-listing-form");
    expect(form).toHaveAttribute("data-disabled", "false");
    expect(form).toHaveTextContent("form-enabled");
  });

  it("guest warning contains links to login and register", () => {
    mockUseAppSelector.mockReturnValue(null);
    render(<CreateListing />);

    // The Trans component is mocked to just display the i18nKey
    expect(screen.getByText("listings:createPage.guestWarning")).toBeInTheDocument();
  });
});
