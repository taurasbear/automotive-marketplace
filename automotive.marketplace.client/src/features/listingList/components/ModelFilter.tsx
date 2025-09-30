import { getModelsByMakeIdOptions } from "@/api/model/getModelsByMakeIdOptions";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { CheckedState } from "@radix-ui/react-checkbox";
import { useQuery } from "@tanstack/react-query";

type ModelFilterProps = {
  makeId: string;
  filteredModels: string[];
  onFilterChange: (value: string[]) => Promise<void>;
};

const ModelFilter = ({
  makeId,
  filteredModels,
  onFilterChange,
}: ModelFilterProps) => {
  const { data: modelsQuery } = useQuery({
    ...getModelsByMakeIdOptions({ makeId }),
    enabled: !!makeId,
  });

  const models = modelsQuery?.data || [];

  const handleCheckedChange = async (
    checked: CheckedState,
    modelId: string,
  ) => {
    const updatedFilteredModels =
      checked === true
        ? [...filteredModels, modelId]
        : filteredModels.filter((value) => value !== modelId);

    await onFilterChange(updatedFilteredModels);
  };

  return (
    <div className="columns-1 space-y-6">
      {models.map((model) => (
        <div key={model.id} className="flex items-center space-x-4">
          <Checkbox
            id={model.id}
            value={model.id}
            checked={filteredModels.includes(model.id)}
            onCheckedChange={(checked) =>
              handleCheckedChange(checked, model.id)
            }
          />
          <Label htmlFor={model.id}>{model.name}</Label>
        </div>
      ))}
    </div>
  );
};

export default ModelFilter;
