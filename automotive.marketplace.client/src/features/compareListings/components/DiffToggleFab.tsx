import { Button } from "@/components/ui/button";

type DiffToggleFabProps = {
  active: boolean;
  onToggle: () => void;
};

export const DiffToggleFab = ({ active, onToggle }: DiffToggleFabProps) => (
  <Button
    onClick={onToggle}
    variant={active ? "default" : "outline"}
    className="fixed bottom-6 right-6 z-20 shadow-lg"
  >
    {active ? "Show All" : "Diff Only"}
  </Button>
);
