import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/api/make/getAllMakesOptions", () => ({
  getAllMakesOptions: {
    queryKey: ["makes"],
    queryFn: () =>
      Promise.resolve({
        data: [
          { id: "make-1", name: "Toyota" },
          { id: "make-2", name: "BMW" },
        ],
      }),
  },
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

import MakeSelect from "./MakeSelect";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("MakeSelect", () => {
  it("renders without crashing", () => {
    render(
      <MakeSelect isAllMakesEnabled={false} label="Make" />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("Make")).toBeInTheDocument();
  });

  it("shows placeholder text", () => {
    render(
      <MakeSelect isAllMakesEnabled={false} />,
      { wrapper: createWrapper() },
    );
    expect(screen.getByText("Toyota")).toBeInTheDocument();
  });

  it("renders makes from query", async () => {
    render(
      <MakeSelect isAllMakesEnabled={false} />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByTestId("select-item-make-1")).toHaveTextContent("Toyota");
    expect(await screen.findByTestId("select-item-make-2")).toHaveTextContent("BMW");
  });

  it("shows 'All Makes' option when isAllMakesEnabled is true", async () => {
    render(
      <MakeSelect isAllMakesEnabled={true} />,
      { wrapper: createWrapper() },
    );
    expect(await screen.findByText("select.allMakes")).toBeInTheDocument();
  });

  it("does not show 'All Makes' option when isAllMakesEnabled is false", async () => {
    render(
      <MakeSelect isAllMakesEnabled={false} />,
      { wrapper: createWrapper() },
    );
    await screen.findByTestId("select-item-make-1");
    expect(screen.queryByText("select.allMakes")).not.toBeInTheDocument();
  });
});
