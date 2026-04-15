import { getVariantsByModelOptions } from "@/features/variantList/api/getVariantsByModelOptions";
import { useQuery } from "@tanstack/react-query";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

type VariantSelectProps = {
  modelId: string;
  year?: number;
  value?: string;
  onValueChange?: (value: string) => void;
  disabled?: boolean;
};

const VariantSelect = ({ modelId, year, value, onValueChange, disabled }: VariantSelectProps) => {
  const { data: variantsQuery, isPending, isError } = useQuery(getVariantsByModelOptions({ modelId, year }));
  const variants = (variantsQuery?.data || []).filter((v) => !year || v.year === year);

  return (
    <Select value={value} onValueChange={onValueChange} disabled={disabled || !modelId || isPending}>
      <SelectTrigger>
        <SelectValue placeholder={isPending ? "Loading variants…" : "Select variant (optional)"} />
      </SelectTrigger>
      <SelectContent>
        {isError && <SelectItem value="__error__" disabled>Failed to load variants</SelectItem>}
        {!isPending && !isError && variants.length === 0 && (
          <SelectItem value="__empty__" disabled>No variants available</SelectItem>
        )}
        {variants.map((v) => (
          <SelectItem key={v.id} value={v.id}>
            {v.year} — {v.fuelName} {v.transmissionName} {v.powerKw}kW {v.engineSizeMl}ml
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
};

export default VariantSelect;
