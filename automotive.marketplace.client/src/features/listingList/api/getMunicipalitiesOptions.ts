import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";

export type MunicipalityOption = {
  id: string;
  name: string;
};

const getMunicipalities = () =>
  axiosClient.get<MunicipalityOption[]>(ENDPOINTS.MUNICIPALITY.GET_ALL);

export const getMunicipalitiesOptions = () =>
  queryOptions({
    queryKey: ["municipalities"],
    queryFn: getMunicipalities,
    staleTime: Infinity,
  });
