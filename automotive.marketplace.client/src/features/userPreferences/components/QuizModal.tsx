import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import {
  Car,
  Gauge,
  Compass,
  BadgeDollarSign,
  Leaf,
  ShieldCheck,
  TrendingDown,
  ShieldAlert,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useUpsertUserPreferences } from "../api/useUpsertUserPreferences";
import { getUserPreferencesOptions } from "../api/getUserPreferencesOptions";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialWeights?: {
    valueWeight: number;
    efficiencyWeight: number;
    reliabilityWeight: number;
    mileageWeight: number;
    conditionWeight: number;
  };
  initialStep?: number;
};

type DrivingStyle = "city" | "highway" | "mixed";
type Priority =
  | "value"
  | "efficiency"
  | "reliability"
  | "mileage"
  | "condition";

const STYLE_ADJUSTMENTS: Record<DrivingStyle, Record<Priority, number>> = {
  city: {
    efficiency: 15,
    reliability: 10,
    value: -15,
    mileage: -10,
    condition: 0,
  },
  highway: {
    efficiency: 15,
    value: 10,
    reliability: -15,
    mileage: -10,
    condition: 0,
  },
  mixed: { efficiency: 0, value: 0, reliability: 0, mileage: 0, condition: 0 },
};

const PRIORITY_BONUSES = [15, 10, 5, 3, 0];

function computeSliders(
  style: DrivingStyle,
  priorities: Priority[],
): Record<Priority, number> {
  const base: Record<Priority, number> = {
    value: 20,
    efficiency: 20,
    reliability: 20,
    mileage: 20,
    condition: 20,
  };
  const adj = STYLE_ADJUSTMENTS[style];
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.max(5, Math.min(60, base[k] + (adj[k] ?? 0)));
  });
  priorities.forEach((p, i) => {
    base[p] = Math.min(60, base[p] + (PRIORITY_BONUSES[i] ?? 0));
  });
  const total = Object.values(base).reduce((a, b) => a + b, 0);
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.round((base[k] / total) * 100);
  });
  return base;
}

function normalizeSliders(
  sliders: Record<Priority, number>,
): Record<Priority, number> {
  const total = Object.values(sliders).reduce((a, b) => a + b, 0);
  if (total === 0)
    return {
      value: 20,
      efficiency: 20,
      reliability: 20,
      mileage: 20,
      condition: 20,
    };
  const result = { ...sliders };
  (Object.keys(result) as Priority[]).forEach((k) => {
    result[k] = Math.round((result[k] / total) * 100);
  });
  return result;
}

export function QuizModal({
  open,
  onOpenChange,
  initialWeights,
  initialStep,
}: Props) {
  const { t } = useTranslation("userPreferences");
  const [step, setStep] = useState(initialStep ?? 0);
  const [drivingStyle, setDrivingStyle] = useState<DrivingStyle>("mixed");
  const [priorities, setPriorities] = useState<Priority[]>([]);
  const [sliders, setSliders] = useState<Record<Priority, number>>(() => {
    if (initialWeights) {
      return {
        value: Math.round(initialWeights.valueWeight * 100),
        efficiency: Math.round(initialWeights.efficiencyWeight * 100),
        reliability: Math.round(initialWeights.reliabilityWeight * 100),
        mileage: Math.round(initialWeights.mileageWeight * 100),
        condition: Math.round(initialWeights.conditionWeight * 100),
      };
    }
    return {
      value: 20,
      efficiency: 20,
      reliability: 20,
      mileage: 20,
      condition: 20,
    };
  });

  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const { mutateAsync: upsert, isPending } = useUpsertUserPreferences();

  const DRIVING_STYLES = [
    {
      id: "city" as DrivingStyle,
      label: t("quiz.styleCity"),
      icon: Car,
      description: t("quiz.styleCityDesc"),
    },
    {
      id: "highway" as DrivingStyle,
      label: t("quiz.styleHighway"),
      icon: Gauge,
      description: t("quiz.styleHighwayDesc"),
    },
    {
      id: "mixed" as DrivingStyle,
      label: t("quiz.styleMixed"),
      icon: Compass,
      description: t("quiz.styleMixedDesc"),
    },
  ];

  const PRIORITIES = [
    {
      id: "value" as Priority,
      label: t("quiz.priorityValue"),
      icon: BadgeDollarSign,
    },
    {
      id: "efficiency" as Priority,
      label: t("quiz.priorityEfficiency"),
      icon: Leaf,
    },
    {
      id: "reliability" as Priority,
      label: t("quiz.priorityReliability"),
      icon: ShieldCheck,
    },
    {
      id: "mileage" as Priority,
      label: t("quiz.priorityMileage"),
      icon: TrendingDown,
    },
    {
      id: "condition" as Priority,
      label: t("quiz.priorityCondition"),
      icon: ShieldAlert,
    },
  ];

  const handleStyleSelect = (style: DrivingStyle) => setDrivingStyle(style);

  const handlePriorityToggle = (priority: Priority) => {
    setPriorities((prev) =>
      prev.includes(priority)
        ? prev.filter((p) => p !== priority)
        : [...prev, priority],
    );
  };

  const handleNextToStep2 = () => setStep(1);

  const handleNextToStep3 = () => {
    setSliders(computeSliders(drivingStyle, priorities));
    setStep(2);
  };

  const handleSliderChange = (key: Priority, value: number) => {
    setSliders((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    const normalized = normalizeSliders(sliders);
    const total = Object.values(normalized).reduce((a, b) => a + b, 0);
    const fraction = (v: number) => Math.round((v / total) * 1000) / 1000;
    const prefs = prefsData?.data;
    await upsert({
      valueWeight: fraction(normalized.value),
      efficiencyWeight: fraction(normalized.efficiency),
      reliabilityWeight: fraction(normalized.reliability),
      mileageWeight: fraction(normalized.mileage),
      conditionWeight: fraction(normalized.condition),
      autoGenerateAiSummary: prefs?.autoGenerateAiSummary ?? false,
      enableVehicleScoring: prefs?.enableVehicleScoring ?? false,
    });
    onOpenChange(false);
    setStep(initialStep ?? 0);
  };

  const sliderTotal = Object.values(sliders).reduce((a, b) => a + b, 0);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>
            {step === 0 && t("quiz.step0Title")}
            {step === 1 && t("quiz.step1Title")}
            {step === 2 && t("quiz.step2Title")}
          </DialogTitle>
        </DialogHeader>

        {step === 0 && (
          <div className="grid grid-cols-3 gap-3">
            {DRIVING_STYLES.map(({ id, label, icon: Icon, description }) => (
              <button
                key={id}
                onClick={() => handleStyleSelect(id)}
                className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                  drivingStyle === id
                    ? "border-primary bg-primary/5"
                    : "border-border hover:border-muted-foreground"
                }`}
              >
                <Icon className="h-6 w-6" />
                <span className="text-sm font-medium">{label}</span>
                <span className="text-muted-foreground text-xs">
                  {description}
                </span>
              </button>
            ))}
          </div>
        )}

        {step === 1 && (
          <div className="grid grid-cols-2 gap-3">
            {PRIORITIES.map(({ id, label, icon: Icon }) => {
              const rank = priorities.indexOf(id);
              const isSelected = rank !== -1;
              return (
                <button
                  key={id}
                  onClick={() => handlePriorityToggle(id)}
                  className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                    isSelected
                      ? "border-primary bg-primary/5"
                      : "border-border hover:border-muted-foreground"
                  }`}
                >
                  <Icon className="h-5 w-5" />
                  <span className="text-sm font-medium">{label}</span>
                  {isSelected && (
                    <span className="bg-primary text-primary-foreground flex h-5 w-5 items-center justify-center rounded-full text-xs">
                      {rank + 1}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        )}

        {step === 2 && (
          <div className="space-y-4">
            <p className="text-muted-foreground text-sm">
              {t("quiz.sliderTotal", { total: sliderTotal })}
              {sliderTotal !== 100 && (
                <span className="ml-1 text-orange-500">
                  {t("quiz.normalizeNotice")}
                </span>
              )}
            </p>
            {PRIORITIES.map(({ id, label }) => (
              <div key={id} className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span>{label}</span>
                  <span className="font-medium">{sliders[id]}%</span>
                </div>
                <Slider
                  min={5}
                  max={60}
                  step={5}
                  value={[sliders[id]]}
                  onValueChange={([v]) => handleSliderChange(id, v)}
                />
              </div>
            ))}
          </div>
        )}

        <DialogFooter className="gap-2">
          {step > 0 && (
            <Button variant="outline" onClick={() => setStep(step - 1)}>
              {t("quiz.back")}
            </Button>
          )}
          {step < 2 && (
            <Button
              onClick={step === 0 ? handleNextToStep2 : handleNextToStep3}
            >
              {t("quiz.next")}
            </Button>
          )}
          {step === 2 && (
            <Button onClick={handleSave} disabled={isPending}>
              {isPending ? t("quiz.saving") : t("quiz.save")}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
