import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { getAllMakesOptions } from "@/shared/utils/queries/make/getAllMakesOptions";
import { useQuery } from "@tanstack/react-query";

type MakeSelectProps = {
  onValueChange: (value: string) => void;
};

const MakeSelect = ({ onValueChange }: MakeSelectProps) => {
  const {
    data: makesQuery,
    isPending: isGetAllMakesPending,
    error: getAllMakesError,
  } = useQuery(getAllMakesOptions);

  if (isGetAllMakesPending) {
    return <h1>Loading...</h1>;
  }

  if (getAllMakesError) {
    return <h1>Error: {getAllMakesError?.message}</h1>;
  }

  const makes = makesQuery?.data;

  return (
    <div>
      <Select defaultValue="all" onValueChange={onValueChange}>
        <SelectTrigger className="w-full">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Makes</SelectLabel>
            <SelectItem value="all">All makes</SelectItem>
            {makes?.map((make) => (
              <SelectItem key={make.id} value={make.id}>
                {make.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default MakeSelect;
