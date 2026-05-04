import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/api/model/getModelsByMakeIdOptions", () => ({
  getModelsByMakeIdOptions: () => ({
    queryKey: ["models", "make-1"],
    queryFn: () =>
      Promise.resolve({
        data: [
          { id: "model-1", name: "Corolla" },
          { id: "model-2", name: "Camry" },
        ],
      }),
  }),
}));

vi.mock("@/components/ui/select", () => ({
  Select: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => (
    <div data-testid="select-root" data-value={props.value}>
      {children}
    </div>
  ),
  SelectContent: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="select-content">{children}</div>
  ),
  SelectGroup: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  SelectItem: ({
    children,
    value,
  }: {
    children: React.ReactNode;
    value: string;
  }) => <div data-testid={`select-item-${value}`}>{children}</div>,
  SelectLabel: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  SelectTrigger: ({
    children,
    ...props
  }: {
    children: React.ReactNode;
    [key: string]: unknown;
  }) => (
    <button aria-label={props["aria-label"] as string}>{children}</button>
  ),
  SelectValue: ({ placeholder }: { placeholder: string }) => (
    <span>{placeholder}</span>
  ),
}));

import ModelSelect from "./ModelSelect";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("ModelSelect", () => {
  it("renders without crashing", () => {
    render(
      <ModelSelect isAllModelsEnabled={false} selectedMake="make-1" label="Model" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("Model")).toBeInTheDocument();
  });

  it("shows placeholder text", () => {
    render(
      <ModelSelect isAllModelsEnabled={false} selectedMake="make-1" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("Auris")).toBeInTheDocument();
  });

  it("renders models from query when make is selected", async () => {
    render(
      <ModelSelect isAllModelsEnabled={false} selectedMake="make-1" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByTestId("select-item-model-1")).toHaveTextContent("Corolla");
    expect(await screen.findByTestId("select-item-model-2")).toHaveTextContent("Camry");
  });

  it("shows 'All Models' option when isAllModelsEnabled is true", async () => {
    render(
      <ModelSelect isAllModelsEnabled={true} selectedMake="make-1" />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByText("select.allModels")).toBeInTheDocument();
  });

  it("does not show 'All Models' option when isAllModelsEnabled is false", async () => {
    render(
      <ModelSelect isAllModelsEnabled={false} selectedMake="make-1" />,
      { wrapper: createWrapper() },
    );
    await screen.findByTestId("select-item-model-1");
    expect(screen.queryByText("select.allModels")).not.toBeInTheDocument();
  });
});
