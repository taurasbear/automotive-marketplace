import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import CreateVariantDialog from "./CreateVariantDialog";

const { mockCreateVariantAsync } = vi.hoisted(() => ({
  mockCreateVariantAsync: vi.fn(),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("../api/useCreateVariant", () => ({
  useCreateVariant: () => ({ mutateAsync: mockCreateVariantAsync }),
}));

vi.mock("./VariantForm", () => ({
  default: ({
    variant,
    onSubmit,
  }: {
    variant: Record<string, unknown>;
    onSubmit: (data: Record<string, unknown>) => void;
  }) => (
    <div data-testid="variant-form">
      <span data-testid="form-modelId">{String(variant.modelId)}</span>
      <span data-testid="form-makeId">{String(variant.makeId)}</span>
      <span data-testid="form-doorCount">{String(variant.doorCount)}</span>
      <button
        data-testid="form-submit"
        onClick={() =>
          onSubmit({
            makeId: "make-1",
            modelId: "model-1",
            fuelId: "fuel-1",
            transmissionId: "trans-1",
            bodyTypeId: "body-1",
            doorCount: 4,
            powerKw: 100,
            engineSizeMl: 1600,
            isCustom: false,
          })
        }
      >
        Submit
      </button>
    </div>
  ),
}));

describe("CreateVariantDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockCreateVariantAsync.mockResolvedValue(undefined);
  });

  it("renders trigger button", () => {
    render(<CreateVariantDialog modelId="model-1" makeId="make-1" />);

    expect(
      screen.getByRole("button", { name: "variants.addVariant" })
    ).toBeInTheDocument();
  });

  it("opens dialog when trigger is clicked", () => {
    render(<CreateVariantDialog modelId="model-1" makeId="make-1" />);

    fireEvent.click(
      screen.getByRole("button", { name: "variants.addVariant" })
    );

    expect(screen.getByText("variants.createNewVariant")).toBeInTheDocument();
  });

  it("renders VariantForm with default values inside dialog", () => {
    render(<CreateVariantDialog modelId="model-1" makeId="make-1" />);

    fireEvent.click(
      screen.getByRole("button", { name: "variants.addVariant" })
    );

    expect(screen.getByTestId("variant-form")).toBeInTheDocument();
    expect(screen.getByTestId("form-modelId")).toHaveTextContent("model-1");
    expect(screen.getByTestId("form-makeId")).toHaveTextContent("make-1");
    expect(screen.getByTestId("form-doorCount")).toHaveTextContent("4");
  });

  it("calls createVariantAsync on form submit", async () => {
    render(<CreateVariantDialog modelId="model-1" makeId="make-1" />);

    fireEvent.click(
      screen.getByRole("button", { name: "variants.addVariant" })
    );
    fireEvent.click(screen.getByTestId("form-submit"));

    expect(mockCreateVariantAsync).toHaveBeenCalledWith({
      modelId: "model-1",
      fuelId: "fuel-1",
      transmissionId: "trans-1",
      bodyTypeId: "body-1",
      doorCount: 4,
      powerKw: 100,
      engineSizeMl: 1600,
      isCustom: false,
    });
  });
});
