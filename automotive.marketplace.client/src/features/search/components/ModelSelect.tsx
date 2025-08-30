import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useQuery } from "@tanstack/react-query";
import { getModelsByMakeIdOptions } from "../api/getModelsByMakeIdOptions";

type ModelSelectProps = {
  selectedMake?: string;
  onValueChange: (value: string) => void;
};

const ModelSelect = ({ selectedMake, onValueChange }: ModelSelectProps) => {
  const { data: modelsQuery } = useQuery({
    ...getModelsByMakeIdOptions({ makeId: selectedMake! }),
    enabled: selectedMake !== undefined,
  });

  const models = modelsQuery?.data || [];

  return (
    <div>
      <Select defaultValue="all" onValueChange={onValueChange}>
        <SelectTrigger className="w-full">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Models</SelectLabel>
            <SelectItem value="all">All models</SelectItem>
            {models.map((model) => (
              <SelectItem key={model.id} value={model.id}>
                {model.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default ModelSelect;
