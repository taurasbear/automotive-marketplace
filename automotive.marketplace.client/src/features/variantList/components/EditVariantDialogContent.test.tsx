import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import EditVariantDialogContent from "./EditVariantDialogContent";
import type { Variant } from "../types/Variant";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/components/ui/dialog", () => ({
  DialogHeader: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="dialog-header">{children}</div>
  ),
  DialogTitle: ({ children }: { children: React.ReactNode }) => (
    <h2 data-testid="dialog-title">{children}</h2>
  ),
  DialogDescription: ({ children }: { children: React.ReactNode }) => (
    <p data-testid="dialog-description">{children}</p>
  ),
}));

vi.mock("./VariantForm", () => ({
  default: ({
    variant,
    onSubmit,
  }: {
    variant: Record<string, unknown>;
    onSubmit: (data: unknown) => void;
  }) => (
    <div data-testid="variant-form">
      <span data-testid="form-makeId">{String(variant.makeId)}</span>
      <span data-testid="form-modelId">{String(variant.modelId)}</span>
      <span data-testid="form-fuelId">{String(variant.fuelId)}</span>
      <span data-testid="form-doorCount">{String(variant.doorCount)}</span>
      <button
        data-testid="form-submit"
        onClick={() => onSubmit({ doorCount: 5 })}
      >
        Submit
      </button>
    </div>
  ),
}));

const mockVariant: Variant = {
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
};

describe("EditVariantDialogContent", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders dialog title", () => {
    render(
      <EditVariantDialogContent
        variant={mockVariant}
        makeId="make-1"
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByText("variants.editVariant")).toBeInTheDocument();
  });

  it("renders VariantForm with correct data", () => {
    render(
      <EditVariantDialogContent
        variant={mockVariant}
        makeId="make-1"
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByTestId("variant-form")).toBeInTheDocument();
    expect(screen.getByTestId("form-makeId")).toHaveTextContent("make-1");
    expect(screen.getByTestId("form-modelId")).toHaveTextContent("model-1");
    expect(screen.getByTestId("form-fuelId")).toHaveTextContent("fuel-1");
    expect(screen.getByTestId("form-doorCount")).toHaveTextContent("4");
  });
});
