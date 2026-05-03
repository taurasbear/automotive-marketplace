import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import VariantListTable from "./VariantListTable";

const { mockDeleteVariantAsync } = vi.hoisted(() => ({
  mockDeleteVariantAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockVariants = [
  {
    id: "var-1",
    modelId: "model-1",
    fuelId: "fuel-1",
    fuelName: "Diesel",
    transmissionId: "trans-1",
    transmissionName: "Manual",
    bodyTypeId: "body-1",
    bodyTypeName: "Sedan",
    doorCount: 4,
    powerKw: 110,
    engineSizeMl: 2000,
    isCustom: false,
  },
  {
    id: "var-2",
    modelId: "model-1",
    fuelId: "fuel-2",
    fuelName: "Petrol",
    transmissionId: "trans-2",
    transmissionName: "Automatic",
    bodyTypeId: "body-2",
    bodyTypeName: "SUV",
    doorCount: 5,
    powerKw: 150,
    engineSizeMl: 2500,
    isCustom: true,
  },
];

vi.mock("@/features/variantList/api/getVariantsByModelIdOptions", () => ({
  getVariantsByModelIdOptions: () => ({
    queryKey: ["variants", "model-1"],
    queryFn: () => Promise.resolve({ data: mockVariants }),
  }),
}));

vi.mock("../api/useDeleteVariant", () => ({
  useDeleteVariant: () => ({ mutateAsync: mockDeleteVariantAsync }),
}));

vi.mock("./EditVariantDialog", () => ({
  default: ({ variant }: { variant: { id: string } }) => (
    <button data-testid={`edit-${variant.id}`}>Edit</button>
  ),
}));

vi.mock("./ViewVariantDialog", () => ({
  default: ({ variant }: { variant: { id: string } }) => (
    <button data-testid={`view-${variant.id}`}>View</button>
  ),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("VariantListTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockDeleteVariantAsync.mockResolvedValue(undefined);
  });

  it("renders table headers", async () => {
    render(<VariantListTable modelId="model-1" makeId="make-1" />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByText("variants.fuel")).toBeInTheDocument();
    expect(screen.getByText("variants.transmission_col")).toBeInTheDocument();
    expect(screen.getByText("variants.bodyType_col")).toBeInTheDocument();
    expect(screen.getByText("variants.doors_col")).toBeInTheDocument();
    expect(screen.getByText("variants.powerKw_col")).toBeInTheDocument();
    expect(screen.getByText("variants.engineMl_col")).toBeInTheDocument();
    expect(screen.getByText("variants.custom")).toBeInTheDocument();
    expect(screen.getByText("variants.actions")).toBeInTheDocument();
  });

  it("renders variant rows after data loads", async () => {
    render(<VariantListTable modelId="model-1" makeId="make-1" />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByText("Diesel")).toBeInTheDocument();
    expect(screen.getByText("Petrol")).toBeInTheDocument();
    expect(screen.getByText("Manual")).toBeInTheDocument();
    expect(screen.getByText("Automatic")).toBeInTheDocument();
  });

  it("renders view and edit buttons for each variant", async () => {
    render(<VariantListTable modelId="model-1" makeId="make-1" />, {
      wrapper: createWrapper(),
    });

    expect(await screen.findByTestId("view-var-1")).toBeInTheDocument();
    expect(screen.getByTestId("edit-var-1")).toBeInTheDocument();
    expect(screen.getByTestId("view-var-2")).toBeInTheDocument();
    expect(screen.getByTestId("edit-var-2")).toBeInTheDocument();
  });

  it("shows custom status text", async () => {
    render(<VariantListTable modelId="model-1" makeId="make-1" />, {
      wrapper: createWrapper(),
    });

    await screen.findByText("Diesel");
    expect(screen.getByText("variants.no")).toBeInTheDocument();
    expect(screen.getByText("variants.yes")).toBeInTheDocument();
  });

  it("calls deleteVariantAsync when delete button is clicked", async () => {
    render(<VariantListTable modelId="model-1" makeId="make-1" />, {
      wrapper: createWrapper(),
    });

    await screen.findByText("Diesel");
    const deleteButtons = screen.getAllByRole("button", { name: "" });
    fireEvent.click(deleteButtons[0]);

    expect(mockDeleteVariantAsync).toHaveBeenCalledWith({ id: "var-1" });
  });
});
