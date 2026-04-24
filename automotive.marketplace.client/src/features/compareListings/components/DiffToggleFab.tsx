import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";

type DiffToggleFabProps = {
  active: boolean;
  onToggle: () => void;
};

export const DiffToggleFab = ({ active, onToggle }: DiffToggleFabProps) => {
  const { t } = useTranslation("compare");

  return (
    <Button
      onClick={onToggle}
      variant={active ? "default" : "outline"}
      className="fixed right-6 bottom-6 z-20 shadow-lg"
    >
      {active ? t("diffToggle.showAll") : t("diffToggle.diffOnly")}
    </Button>
  );
};
