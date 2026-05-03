import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import ContractFormDialog from "./ContractFormDialog";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/dateLocale", () => ({
  useDateLocale: () => undefined,
}));

vi.mock("../api/getUserContractProfileOptions", () => ({
  getUserContractProfileOptions: () => ({
    queryKey: ["userContractProfile"],
    queryFn: () => Promise.resolve({ data: null }),
  }),
}));

const mockOnSubmitSeller = vi.fn();
const mockOnSubmitBuyer = vi.fn();
const mockOnOpenChange = vi.fn();

beforeEach(() => {
  vi.clearAllMocks();
});

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

const defaultProps = {
  open: true,
  onOpenChange: mockOnOpenChange,
  contractCardId: "card-1",
  isSeller: true,
  vehicleDefaults: {
    make: "BMW",
    commercialName: "3 Series",
    vin: "WBA12345678901234",
    mileage: 50000,
    price: 25000,
  },
  userEmail: "test@example.com",
  onSubmitSeller: mockOnSubmitSeller,
  onSubmitBuyer: mockOnSubmitBuyer,
};

describe("ContractFormDialog", () => {
  it("renders dialog with title when open", () => {
    render(<ContractFormDialog {...defaultProps} />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("contractForm.title")).toBeInTheDocument();
  });

  it("does not render when closed", () => {
    render(<ContractFormDialog {...defaultProps} open={false} />, {
      wrapper: createWrapper(),
    });
    expect(screen.queryByText("contractForm.title")).not.toBeInTheDocument();
  });

  it("shows step 1 vehicle fields for seller", () => {
    render(<ContractFormDialog {...defaultProps} />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("contractForm.make")).toBeInTheDocument();
    expect(screen.getByText("contractForm.vin")).toBeInTheDocument();
    expect(screen.getByText("contractForm.mileage")).toBeInTheDocument();
  });

  it("shows step indicator for seller", () => {
    render(<ContractFormDialog {...defaultProps} />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("contractForm.step1")).toBeInTheDocument();
    expect(screen.getByText("contractForm.step2")).toBeInTheDocument();
  });

  it("shows step 2 personal fields for buyer directly", () => {
    render(<ContractFormDialog {...defaultProps} isSeller={false} />, {
      wrapper: createWrapper(),
    });
    expect(screen.getByText("contractForm.fullName")).toBeInTheDocument();
    expect(screen.getByText("contractForm.phone")).toBeInTheDocument();
    expect(screen.getByText("contractForm.address")).toBeInTheDocument();
  });

  it("navigates to step 2 when next button clicked (seller)", async () => {
    const user = userEvent.setup();
    render(<ContractFormDialog {...defaultProps} />, {
      wrapper: createWrapper(),
    });
    await user.click(screen.getByText("contractForm.next"));
    expect(screen.getByText("contractForm.fullName")).toBeInTheDocument();
  });

  it("shows read-only title when isReadOnly", () => {
    render(
      <ContractFormDialog
        {...defaultProps}
        isReadOnly={true}
        submittedAt="2025-01-15T00:00:00Z"
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("contractForm.titleReadOnly")).toBeInTheDocument();
  });

  it("hides submit button when isReadOnly", () => {
    render(
      <ContractFormDialog
        {...defaultProps}
        isSeller={false}
        isReadOnly={true}
        submittedAt="2025-01-15T00:00:00Z"
      />,
      { wrapper: createWrapper() },
    );
    expect(screen.queryByText("contractForm.submit")).not.toBeInTheDocument();
  });
});
